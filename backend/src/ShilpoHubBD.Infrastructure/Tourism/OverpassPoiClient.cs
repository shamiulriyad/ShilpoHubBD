using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Infrastructure.Tourism;

/// <summary>
/// Reads accommodation and points of interest around a coordinate from OpenStreetMap through the
/// Overpass API -- ONE query for every category, then a filtering layer that keeps only useful,
/// named, tourism-relevant objects. OSM is community data: nothing is invented, a field the object
/// does not carry stays null, and everything returned is unverified until an admin says otherwise.
/// Throws on failure (after the configured retries) so the caller can fall back to stored data.
/// </summary>
public class OverpassPoiClient : IExternalPoiClient
{
    // Kept as data so the query and the classifier stay in one obvious place.
    private static readonly string[] AccommodationTags = ["hotel", "hostel", "resort", "guest_house", "motel", "apartment", "camp_site"];
    private static readonly string[] AttractionTags = ["attraction", "museum", "theme_park", "viewpoint", "zoo", "gallery"];
    // historic=* is huge and mostly noise (boundary stones, wayside shrines); only these are places to visit.
    private static readonly HashSet<string> VisitableHistoric = new(StringComparer.OrdinalIgnoreCase)
    {
        "archaeological_site", "castle", "fort", "monument", "memorial", "ruins", "palace", "manor", "building",
        "church", "monastery", "mosque", "temple", "tomb", "city_gate", "battlefield", "citywalls", "ship", "railway_station",
    };
    // Every village has a mosque; only worship places that OSM itself flags as notable are worth listing.
    private static readonly string[] NotableWorshipTags = ["wikidata", "wikipedia", "heritage", "historic", "tourism"];

    private readonly HttpClient _httpClient;
    private readonly ExternalPoiOptions _options;
    private readonly ILogger<OverpassPoiClient> _logger;

    public OverpassPoiClient(HttpClient httpClient, IOptions<ExternalPoiOptions> options, ILogger<OverpassPoiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<ExternalPoiDto>> FetchAsync(double latitude, double longitude, double radiusKm, ExternalPoiScope scope, CancellationToken cancellationToken)
    {
        var elements = new List<OverpassElement>();
        Exception? last = null;
        var succeeded = 0;

        foreach (var query in BuildQueries(scope, latitude, longitude, radiusKm))
        {
            try
            {
                elements.AddRange(await RunQueryAsync(query, cancellationToken));
                succeeded++;
            }
            catch (Exception exc) when (exc is HttpRequestException && !cancellationToken.IsCancellationRequested)
            {
                last = exc;   // this group failed; keep whatever the others return
            }
        }

        if (succeeded == 0)
        {
            throw new HttpRequestException("Overpass is unavailable.", last);
        }

        return Normalize(elements, scope == ExternalPoiScope.Accommodation ? Math.Max(_options.MaxPerCategory, 300) : _options.MaxPerCategory);
    }

    // One query, tried against each endpoint (primary, then mirrors) and retried per RetryCount.
    private async Task<List<OverpassElement>> RunQueryAsync(string query, CancellationToken cancellationToken)
    {
        Exception? last = null;
        var endpoints = new[] { _options.OverpassUrl }.Concat(_options.FallbackOverpassUrls).Where(u => !string.IsNullOrWhiteSpace(u)).ToList();

        for (var attempt = 0; attempt <= Math.Max(0, _options.RetryCount); attempt++)
        {
            foreach (var endpoint in endpoints)
            {
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(TimeSpan.FromSeconds(_options.TimeoutSeconds));

                    using var request = new HttpRequestMessage(HttpMethod.Post, endpoint)
                    {
                        Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["data"] = query }),
                    };
                    request.Headers.UserAgent.ParseAdd(_options.UserAgent);

                    using var response = await _httpClient.SendAsync(request, timeout.Token);
                    // 429 / 5xx: Overpass says "busy" -- move on to the next mirror / attempt.
                    if ((int)response.StatusCode is 429 or 502 or 503 or 504)
                    {
                        throw new HttpRequestException($"Overpass busy ({(int)response.StatusCode}).");
                    }
                    response.EnsureSuccessStatusCode();

                    var body = await response.Content.ReadFromJsonAsync<OverpassResponse>(cancellationToken: timeout.Token);
                    return body?.Elements ?? [];
                }
                catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException && !cancellationToken.IsCancellationRequested)
                {
                    last = exc;
                    _logger.LogWarning(exc, "Overpass request to {Endpoint} failed (attempt {Attempt}).", endpoint, attempt + 1);
                }
            }

            if (attempt < _options.RetryCount)
            {
                await Task.Delay(TimeSpan.FromSeconds(2 * (attempt + 1)), cancellationToken);
            }
        }

        throw new HttpRequestException("Overpass is unavailable.", last);
    }

    // Small queries, one per group. `nwr` covers nodes, ways and relations; `out center` gives ways a point.
    internal static List<string> BuildQueries(ExternalPoiScope scope, double lat, double lon, double radiusKm)
    {
        var around = string.Create(CultureInfo.InvariantCulture, $"(around:{(int)(radiusKm * 1000)},{lat:F5},{lon:F5})");
        static string Wrap(string body, int limit) => $"[out:json][timeout:20];\n(\n{body}\n);\nout center tags {limit};";
        var queries = new List<string>();

        if (scope is ExternalPoiScope.All or ExternalPoiScope.Accommodation)
        {
            queries.Add(Wrap($"  nwr[\"tourism\"~\"^(hotel|hostel|resort|guest_house|motel)$\"][\"name\"]{around};", 800));
        }

        if (scope is ExternalPoiScope.All or ExternalPoiScope.Places)
        {
            queries.Add(Wrap($"  nwr[\"amenity\"~\"^(restaurant|cafe)$\"][\"name\"]{around};", 800));

            var historic = string.Join("|", VisitableHistoric);
            queries.Add(Wrap(string.Join("\n", new[]
            {
                $"  nwr[\"tourism\"~\"^({string.Join("|", AttractionTags)})$\"][\"name\"]{around};",
                $"  nwr[\"leisure\"=\"park\"][\"name\"]{around};",
                $"  nwr[\"historic\"~\"^({historic})$\"][\"name\"]{around};",
                $"  nwr[\"natural\"~\"^(beach|waterfall|hot_spring)$\"][\"name\"]{around};",
            }), 800));

            // Every village has a mosque; only worship places OSM itself flags as notable are worth listing.
            queries.Add(Wrap(string.Join("\n", NotableWorshipTags.Select(t =>
                $"  nwr[\"amenity\"=\"place_of_worship\"][\"name\"][\"{t}\"]{around};")), 300));
        }

        return queries;
    }

    // The filtering layer. Public-internal so it can be exercised without a network call.
    internal static List<ExternalPoiDto> Normalize(IEnumerable<OverpassElement> elements, int maxPerCategory)
    {
        var result = new Dictionary<string, ExternalPoiDto>();

        foreach (var element in elements)
        {
            var tags = element.Tags;
            if (tags is null || !tags.TryGetValue("name", out var name) || string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            var lat = element.Lat ?? element.Center?.Lat;
            var lon = element.Lon ?? element.Center?.Lon;
            var type = Classify(tags);
            if (type is TourismLocationType.Hotel or TourismLocationType.Motel or TourismLocationType.Hostel or TourismLocationType.GuestHouse or TourismLocationType.Resort
                && LooksLikeEatery(name))
            {
                continue;
            }

            if (lat is null || lon is null || type is null)
            {
                continue;
            }

            var externalId = $"{element.Type}/{element.Id}";
            result[externalId] = new ExternalPoiDto
            {
                ExternalId = externalId,
                Name = name.Trim(),
                Type = type.Value,
                Latitude = lat.Value,
                Longitude = lon.Value,
                Address = BuildAddress(tags),
                Area = First(tags, "addr:suburb", "addr:neighbourhood", "addr:village", "addr:city", "is_in:city"),
                Upazila = First(tags, "addr:subdistrict", "addr:upazila", "is_in:subdistrict"),
                Phone = First(tags, "phone", "contact:phone", "contact:mobile"),
                Website = First(tags, "website", "contact:website"),
                OpeningHours = First(tags, "opening_hours"),
                Description = First(tags, "description:en", "description"),
                TagCount = tags.Count,
            };
        }

        // Keep the best-documented objects per category; drop a second listing of the same name at the
        // same spot (OSM often maps one place as both a node and a building way).
        return result.Values
            .GroupBy(p => p.Type)
            .SelectMany(g => g
                .OrderByDescending(p => p.TagCount).ThenBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .DistinctBy(p => (p.Name.ToLowerInvariant(), Math.Round(p.Latitude, 3), Math.Round(p.Longitude, 3)))
                .Take(maxPerCategory))
            .ToList();
    }

    // In Bangladesh "hotel" is very often an eatery (a kabab house, a biryani shop) and OSM mappers
    // sometimes tag those tourism=hotel. A lodging whose own name says it serves food is not somewhere
    // to sleep, so it is left out rather than shown as accommodation.
    private static readonly string[] EateryWords =
        ["kabab", "kebab", "biriyani", "biryani", "foods", "food ", " food", "dokan", "sweets", "bakery", "canteen", "tea stall", "cafe", "fast food"];

    internal static bool LooksLikeEatery(string name)
        => EateryWords.Any(w => name.Contains(w, StringComparison.OrdinalIgnoreCase));

    internal static TourismLocationType? Classify(Dictionary<string, string> tags)
    {
        if (tags.TryGetValue("tourism", out var tourism))
        {
            switch (tourism)
            {
                case "hotel": return TourismLocationType.Hotel;
                case "resort": return TourismLocationType.Resort;
                case "hostel": return TourismLocationType.Hostel;
                case "guest_house": return TourismLocationType.GuestHouse;
                case "motel": return TourismLocationType.Motel;
                case "apartment":
                case "camp_site": return TourismLocationType.Homestay;
                case "museum":
                case "gallery": return TourismLocationType.Museum;
                case "viewpoint": return TourismLocationType.Viewpoint;
                case "attraction":
                case "theme_park":
                case "zoo": return TourismLocationType.Attraction;
            }
        }

        if (tags.TryGetValue("amenity", out var amenity))
        {
            switch (amenity)
            {
                case "restaurant": return TourismLocationType.Restaurant;
                case "cafe": return TourismLocationType.Cafe;
                case "place_of_worship":
                    if (!NotableWorshipTags.Any(tags.ContainsKey)) return null;
                    return tags.GetValueOrDefault("religion") switch
                    {
                        "muslim" => TourismLocationType.Mosque,
                        "hindu" or "buddhist" => TourismLocationType.Temple,
                        _ => null,
                    };
            }
        }

        if (tags.GetValueOrDefault("leisure") == "park") return TourismLocationType.Park;
        if (tags.GetValueOrDefault("natural") == "beach") return TourismLocationType.Beach;
        if (tags.TryGetValue("natural", out var natural) && natural is "waterfall" or "hot_spring") return TourismLocationType.Attraction;
        if (tags.TryGetValue("historic", out var historic) && VisitableHistoric.Contains(historic)) return TourismLocationType.HistoricalPlace;
        return null;
    }

    private static string? First(Dictionary<string, string> tags, params string[] keys)
        => keys.Select(k => tags.GetValueOrDefault(k)).FirstOrDefault(v => !string.IsNullOrWhiteSpace(v))?.Trim();

    private static string? BuildAddress(Dictionary<string, string> tags)
    {
        var full = First(tags, "addr:full");
        if (full is not null) return full;
        var parts = new[] { First(tags, "addr:housenumber"), First(tags, "addr:street"), First(tags, "addr:suburb"), First(tags, "addr:city") }
            .Where(p => p is not null);
        var joined = string.Join(", ", parts);
        return joined.Length == 0 ? null : joined;
    }

    internal class OverpassResponse
    {
        [JsonPropertyName("elements")] public List<OverpassElement>? Elements { get; set; }
    }

    internal class OverpassElement
    {
        [JsonPropertyName("type")] public string Type { get; set; } = string.Empty;
        [JsonPropertyName("id")] public long Id { get; set; }
        [JsonPropertyName("lat")] public double? Lat { get; set; }
        [JsonPropertyName("lon")] public double? Lon { get; set; }
        [JsonPropertyName("center")] public OverpassCenter? Center { get; set; }
        [JsonPropertyName("tags")] public Dictionary<string, string>? Tags { get; set; }
    }

    internal class OverpassCenter
    {
        [JsonPropertyName("lat")] public double Lat { get; set; }
        [JsonPropertyName("lon")] public double Lon { get; set; }
    }
}
