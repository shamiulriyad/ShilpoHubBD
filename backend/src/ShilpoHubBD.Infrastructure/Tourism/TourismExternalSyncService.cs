using System.Collections.Concurrent;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Infrastructure.Tourism;

/// <summary>
/// Cache-first import of OpenStreetMap accommodation and POIs into TourismLocations.
/// Flow: fresh rows in the database -> return them; stale/none -> geocode the district centre,
/// one Overpass query, normalise, deduplicate against existing records (an admin record always wins),
/// upsert, stamp LastSyncedAt. Any failure is logged and swallowed -- the tourism page keeps working
/// from whatever is already stored.
/// </summary>
public class TourismExternalSyncService : ITourismExternalSyncService
{
    public const string OpenStreetMap = "OpenStreetMap";
    private const double DuplicateDistanceMeters = 150;
    private const double SameNameDistanceMeters = 600;

    // A failed/empty attempt is remembered so a district with no OSM data (or a down Overpass) is not
    // re-queried on every page view. Per-district lock stops two simultaneous views importing twice.
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Locks = new();
    // Largest radius already imported per district and mode, so a wider request triggers a new import.
    private static readonly ConcurrentDictionary<string, double> SyncedRadius = new();

    private readonly ITourismLocationRepository _locations;
    private readonly IDistrictRepository _districts;
    private readonly IGeocodingProvider _geocoder;
    private readonly IExternalPoiClient _client;
    private readonly IMemoryCache _cache;
    private readonly ExternalPoiOptions _options;
    private readonly ILogger<TourismExternalSyncService> _logger;
    private readonly IServiceScopeFactory _scopeFactory;

    public TourismExternalSyncService(
        ITourismLocationRepository locations, IDistrictRepository districts, IGeocodingProvider geocoder,
        IExternalPoiClient client, IMemoryCache cache, IOptions<ExternalPoiOptions> options,
        ILogger<TourismExternalSyncService> logger, IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
        _locations = locations;
        _districts = districts;
        _geocoder = geocoder;
        _client = client;
        _cache = cache;
        _options = options.Value;
        _logger = logger;
    }

    // Used by page views. The import runs on its own scope and token so it survives the request; the
    // request waits only MaxWaitSeconds for it and otherwise answers with what is stored.
    public async Task<TourismSyncResultDto> EnsureFreshAsync(Guid districtId, CancellationToken cancellationToken, double? radiusKm = null, ExternalPoiScope scope = ExternalPoiScope.All)
    {
        var background = Task.Run(async () =>
        {
            using var serviceScope = _scopeFactory.CreateScope();
            var sync = serviceScope.ServiceProvider.GetRequiredService<TourismExternalSyncService>();
            try
            {
                return await sync.SyncAsync(districtId, force: false, CancellationToken.None, radiusKm, scope);
            }
            catch (Exception exc)
            {
                _logger.LogWarning(exc, "Background OpenStreetMap sync failed for district {DistrictId}.", districtId);
                return new TourismSyncResultDto { DistrictId = districtId, Message = "Sync failed." };
            }
        }, CancellationToken.None);

        var finished = await Task.WhenAny(background, Task.Delay(TimeSpan.FromSeconds(_options.MaxWaitSeconds), cancellationToken));
        return finished == background
            ? await background
            : new TourismSyncResultDto { DistrictId = districtId, Attempted = true, Message = "Import continues in the background." };
    }

    public async Task<TourismSyncResultDto> SyncAsync(Guid districtId, bool force, CancellationToken cancellationToken, double? radiusKm = null, ExternalPoiScope scope = ExternalPoiScope.All)
    {
        var radius = Math.Clamp(radiusKm ?? _options.DefaultRadiusKm, 1, _options.MaxRadiusKm);
        var mode = scope.ToString();
        var key = $"{districtId}:{mode}";
        IReadOnlyCollection<TourismLocationType>? freshnessTypes = scope switch
        {
            ExternalPoiScope.Accommodation => AccommodationService.AccommodationTypes,
            ExternalPoiScope.Places => TourismPoiService.PoiTypes,
            _ => null,
        };

        var district = await _districts.GetByIdAsync(districtId, cancellationToken)
            ?? throw new NotFoundException("District not found.");
        var result = new TourismSyncResultDto { DistrictId = districtId, DistrictName = district.Name };

        if (!_options.Enabled)
        {
            result.Message = "External POI import is disabled.";
            return result;
        }

        var gate = Locks.GetOrAdd(key, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var attemptKey = $"tourism-sync-attempt:{key}:{(int)radius}";
            if (!force)
            {
                var lastSynced = await _locations.GetLastSyncedAtAsync(districtId, freshnessTypes, cancellationToken);
                var coveredRadius = SyncedRadius.GetValueOrDefault(key, _options.DefaultRadiusKm);
                if (lastSynced.HasValue && DateTime.UtcNow - lastSynced.Value < TimeSpan.FromDays(_options.RefreshDays) && coveredRadius >= radius)
                {
                    result.Message = "Stored data is fresh.";
                    result.Succeeded = true;
                    return result;
                }

                if (_cache.TryGetValue(attemptKey, out _))
                {
                    result.Message = "A recent import attempt found nothing new.";
                    return result;
                }
            }

            result.Attempted = true;
            try
            {
                await ImportAsync(district.Name, districtId, radius, scope, result, cancellationToken);
                result.Succeeded = true;
                if (result.Fetched > 0) SyncedRadius[key] = Math.Max(radius, SyncedRadius.GetValueOrDefault(key, 0));
                // Nothing came back: do not hammer Overpass again for a while (rows, if any, stay served).
                if (result.Fetched == 0)
                {
                    _cache.Set(attemptKey, true, TimeSpan.FromMinutes(_options.RetryAfterMinutes));
                }
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exc)
            {
                _logger.LogWarning(exc, "OpenStreetMap import failed for district {District}; serving stored data.", district.Name);
                _cache.Set(attemptKey, true, TimeSpan.FromMinutes(_options.RetryAfterMinutes));
                result.Message = "OpenStreetMap is unavailable right now; showing stored data.";
            }

            return result;
        }
        finally
        {
            gate.Release();
        }
    }

    private async Task ImportAsync(string districtName, Guid districtId, double radius, ExternalPoiScope scope, TourismSyncResultDto result, CancellationToken cancellationToken)
    {
        var center = await _geocoder.GeocodeAsync($"{districtName} District, Bangladesh", cancellationToken)
            ?? await _geocoder.GeocodeAsync($"{districtName}, Bangladesh", cancellationToken);
        if (center is null)
        {
            result.Message = "Could not locate the district centre, so nothing was imported.";
            return;
        }

        var fetched = await _client.FetchAsync(center.Latitude, center.Longitude, radius, scope, cancellationToken);
        result.Fetched = fetched.Count;

        var existing = await _locations.GetAllForDistrictTrackedAsync(districtId, cancellationToken);
        var osmById = existing.Where(l => l.Source == OpenStreetMap && l.ExternalId != null)
            .ToDictionary(l => l.ExternalId!, StringComparer.Ordinal);
        var others = existing.Where(l => l.Source != OpenStreetMap).ToList();
        var now = DateTime.UtcNow;

        foreach (var poi in fetched)
        {
            if (osmById.TryGetValue(poi.ExternalId, out var row))
            {
                // A row an admin has reviewed keeps the admin's edits; only its sync stamp moves.
                if (!row.IsVerified)
                {
                    Apply(row, poi);
                }
                row.LastSyncedAt = now;
                result.Updated++;
                continue;
            }

            if (others.Any(o => IsSamePlace(o, poi)))
            {
                result.SkippedDuplicates++;   // the admin-curated record already represents it
                continue;
            }

            var entity = new TourismLocation
            {
                Id = Guid.NewGuid(),
                DistrictId = districtId,
                Source = OpenStreetMap,
                ExternalId = poi.ExternalId,
                IsVerified = false,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
                LastSyncedAt = now,
                DataRetrievedOn = now,
                VerificationStatus = "Unverified",
                CoordinatesSource = "OpenStreetMap",
                CoordinatesPrecision = "community_listing",
                SourceUrl = $"https://www.openstreetmap.org/{poi.ExternalId}",
                UnverifiedFields = "price, rating, availability, facilities",
            };
            Apply(entity, poi);
            await _locations.AddAsync(entity, cancellationToken);
            osmById[poi.ExternalId] = entity;
            result.Added++;
        }

        await _locations.SaveChangesAsync(cancellationToken);
        result.Message = $"Imported from OpenStreetMap: {result.Added} new, {result.Updated} refreshed, {result.SkippedDuplicates} already covered by admin records.";
    }

    private static void Apply(TourismLocation row, ExternalPoiDto poi)
    {
        row.Name = poi.Name.Length > 200 ? poi.Name[..200] : poi.Name;
        row.Type = poi.Type;
        row.Latitude = poi.Latitude;
        row.Longitude = poi.Longitude;
        row.Address = poi.Address;
        row.Area = poi.Area;
        row.Upazila = poi.Upazila;
        row.OpeningHours = poi.OpeningHours;
        var contact = string.Join(" | ", new[] { poi.Phone, poi.Website }.Where(v => !string.IsNullOrWhiteSpace(v)));
        row.ContactInfo = contact.Length == 0 ? null : (contact.Length > 300 ? contact[..300] : contact);
        // Required column, but never made up: it says only what the record is and where it came from.
        row.Description = poi.Description is { Length: > 0 }
            ? (poi.Description.Length > 1000 ? poi.Description[..1000] : poi.Description)
            : $"{poi.Type} listed on OpenStreetMap. Details have not been verified by ShilpoHub.";
        row.UpdatedAt = DateTime.UtcNow;
    }

    // Same place if it is very close and the names look alike, or nearly the same name a bit further out.
    private static bool IsSamePlace(TourismLocation existing, ExternalPoiDto poi)
    {
        var meters = DistanceMeters(existing.Latitude, existing.Longitude, poi.Latitude, poi.Longitude);
        var similarity = NameSimilarity(existing.Name, poi.Name);
        return (meters <= DuplicateDistanceMeters && similarity >= 0.5) || (meters <= SameNameDistanceMeters && similarity >= 0.85);
    }

    internal static double NameSimilarity(string a, string b)
    {
        var (ta, tb) = (Tokens(a), Tokens(b));
        if (ta.Count == 0 || tb.Count == 0) return 0;
        var normA = string.Join(" ", ta.OrderBy(t => t));
        var normB = string.Join(" ", tb.OrderBy(t => t));
        if (normA == normB) return 1;
        if (normA.Contains(normB) || normB.Contains(normA)) return 0.9;
        return (double)ta.Intersect(tb).Count() / ta.Union(tb).Count();
    }

    private static readonly HashSet<string> NoiseWords = new(StringComparer.OrdinalIgnoreCase)
        { "the", "of", "and", "hotel", "restaurant", "resort", "cafe", "museum", "park", "mosque", "temple", "masjid", "mandir" };

    private static HashSet<string> Tokens(string name)
    {
        var cleaned = new string(name.ToLowerInvariant().Select(c => char.IsLetterOrDigit(c) ? c : ' ').ToArray());
        var all = cleaned.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var meaningful = all.Where(t => !NoiseWords.Contains(t)).ToHashSet();
        return meaningful.Count > 0 ? meaningful : all.ToHashSet();
    }

    internal static double DistanceMeters(double lat1, double lon1, double lat2, double lon2)
    {
        const double r = 6371000;
        var dLat = (lat2 - lat1) * Math.PI / 180;
        var dLon = (lon2 - lon1) * Math.PI / 180;
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2)
            + Math.Cos(lat1 * Math.PI / 180) * Math.Cos(lat2 * Math.PI / 180) * Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        return 2 * r * Math.Asin(Math.Sqrt(a));
    }
}
