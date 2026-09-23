using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.Geocoding;

/// <summary>
/// Real geocoding via the free, keyless OpenStreetMap Nominatim API -- restricted to Bangladesh
/// since this platform is Bangladesh-focused. Results are cached (Nominatim's usage policy asks
/// callers to cache) and outbound calls are serialised through <see cref="NominatimRateGate"/> so
/// this app never exceeds the ~1 request/second the free public instance expects. Returns null on
/// no match or any failure -- never throws, never guesses.
/// </summary>
public class NominatimGeocodingProvider : IGeocodingProvider
{
    private readonly HttpClient _httpClient;
    private readonly NominatimOptions _options;
    private readonly IMemoryCache _cache;
    private readonly NominatimRateGate _rateGate;
    private readonly ILogger<NominatimGeocodingProvider> _logger;

    public NominatimGeocodingProvider(
        HttpClient httpClient, IOptions<NominatimOptions> options, IMemoryCache cache, NominatimRateGate rateGate,
        ILogger<NominatimGeocodingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _rateGate = rateGate;
        _logger = logger;
    }

    public async Task<GeoPointDto?> GeocodeAsync(string query, CancellationToken cancellationToken)
    {
        var normalized = query.Trim().ToLowerInvariant();
        if (normalized.Length == 0)
        {
            return null;
        }

        var cacheKey = $"nominatim:{normalized}";
        if (_cache.TryGetValue(cacheKey, out GeoPointDto? cached))
        {
            return cached;
        }

        try
        {
            await _rateGate.WaitAsync(cancellationToken);

            var url = $"search?q={Uri.EscapeDataString(query)}&format=json&limit=1&countrycodes=bd";
            using var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.UserAgent.ParseAdd(_options.UserAgent);

            using var response = await _httpClient.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Nominatim returned {StatusCode} for query '{Query}'.", response.StatusCode, query);
                return null;
            }

            var results = await response.Content.ReadFromJsonAsync<List<NominatimResult>>(cancellationToken: cancellationToken);
            var first = results?.FirstOrDefault();
            if (first is null || !double.TryParse(first.Lat, out var lat) || !double.TryParse(first.Lon, out var lon))
            {
                return null;
            }

            var point = new GeoPointDto { Latitude = lat, Longitude = lon, DisplayName = first.DisplayName ?? query };
            _cache.Set(cacheKey, point, TimeSpan.FromHours(_options.CacheHours));
            return point;
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exc, "Geocoding call failed for query '{Query}'.", query);
            return null;
        }
    }

    private class NominatimResult
    {
        [JsonPropertyName("lat")]
        public string? Lat { get; set; }

        [JsonPropertyName("lon")]
        public string? Lon { get; set; }

        [JsonPropertyName("display_name")]
        public string? DisplayName { get; set; }
    }
}
