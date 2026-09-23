using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.Routing;

/// <summary>
/// Real road-network routing via the free, keyless public OSRM demo server (driving profile).
/// Returns the actual distance, duration and route geometry -- never a straight-line guess.
/// Results are cached by rounded coordinate pair, since the demo instance is not meant for heavy
/// production load. Returns null on any failure or "no route found" -- never throws.
/// </summary>
public class OsrmRoutingProvider : IRoutingProvider
{
    private readonly HttpClient _httpClient;
    private readonly OsrmOptions _options;
    private readonly IMemoryCache _cache;
    private readonly ILogger<OsrmRoutingProvider> _logger;

    public OsrmRoutingProvider(HttpClient httpClient, IOptions<OsrmOptions> options, IMemoryCache cache, ILogger<OsrmRoutingProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RouteResultDto?> GetDrivingRouteAsync(GeoPointDto origin, GeoPointDto destination, CancellationToken cancellationToken)
    {
        var cacheKey = "osrm:" + string.Join(',',
            Round(origin.Latitude), Round(origin.Longitude), Round(destination.Latitude), Round(destination.Longitude));
        if (_cache.TryGetValue(cacheKey, out RouteResultDto? cached))
        {
            return cached;
        }

        try
        {
            var coordinates = string.Format(
                CultureInfo.InvariantCulture, "{0},{1};{2},{3}",
                origin.Longitude, origin.Latitude, destination.Longitude, destination.Latitude);
            var url = $"route/v1/driving/{coordinates}?overview=full&geometries=geojson";

            using var response = await _httpClient.GetAsync(url, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("OSRM returned {StatusCode} for a route request.", response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<OsrmResponse>(cancellationToken: cancellationToken);
            var route = body?.Routes?.FirstOrDefault();
            if (body?.Code != "Ok" || route is null)
            {
                return null;
            }

            var result = new RouteResultDto
            {
                DistanceKm = Math.Round(route.Distance / 1000.0, 1),
                DurationMinutes = (int)Math.Round(route.Duration / 60.0),
                Geometry = (route.Geometry?.Coordinates ?? [])
                    .Where(c => c.Count == 2)
                    .Select(c => new GeoPointDto { Longitude = c[0], Latitude = c[1] })
                    .ToList(),
            };

            _cache.Set(cacheKey, result, TimeSpan.FromHours(_options.CacheHours));
            return result;
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exc, "Routing call failed.");
            return null;
        }
    }

    private static double Round(double value) => Math.Round(value, 3);

    private class OsrmResponse
    {
        [JsonPropertyName("code")]
        public string? Code { get; set; }

        [JsonPropertyName("routes")]
        public List<OsrmRoute>? Routes { get; set; }
    }

    private class OsrmRoute
    {
        [JsonPropertyName("distance")]
        public double Distance { get; set; }

        [JsonPropertyName("duration")]
        public double Duration { get; set; }

        [JsonPropertyName("geometry")]
        public OsrmGeometry? Geometry { get; set; }
    }

    private class OsrmGeometry
    {
        [JsonPropertyName("coordinates")]
        public List<List<double>>? Coordinates { get; set; }
    }
}
