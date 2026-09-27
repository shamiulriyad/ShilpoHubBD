using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AITourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.AITourism;

/// <summary>
/// Calls the isolated Travel Planner Knowledge Base (`rag/travel/`, a separate Qdrant
/// collection from the general Heritage Assistant's) over HTTP for real, dataset-grounded
/// tourism context. Falls back to an empty list -- never throws -- so a down RAG service or
/// an un-indexed collection degrades the itinerary (no grounded notes) instead of breaking it.
/// </summary>
public class RagTravelPlannerProvider : ITravelPlannerRagProvider
{
    private readonly HttpClient _httpClient;
    private readonly RagServiceOptions _options;
    private readonly ILogger<RagTravelPlannerProvider> _logger;

    public RagTravelPlannerProvider(HttpClient httpClient, IOptions<RagServiceOptions> options, ILogger<RagTravelPlannerProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<RagTravelNoteDto>> RetrieveAsync(
        string query, string? district, IReadOnlyList<string>? interests, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/kb/{Uri.EscapeDataString(_options.TravelPlannerCollection)}/retrieve",
                new RetrieveRequest(query, district, interests?.ToList()),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Travel Planner RAG returned {StatusCode} for a retrieval request.", response.StatusCode);
                return [];
            }

            var result = await response.Content.ReadFromJsonAsync<RetrieveResponse>(cancellationToken: cancellationToken);
            return (result?.Results ?? [])
                .Select(r => new RagTravelNoteDto { Text = r.Text, PlaceType = r.PlaceType })
                .ToList();
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exc, "Travel Planner RAG call failed.");
            return [];
        }
    }

    public async Task<DistrictDatasetResult> GetDistrictEntitiesAsync(
        string district, IReadOnlyList<string>? interests, int limit, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/travel/entities", new EntitiesRequest(district, interests?.ToList(), limit), cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Travel Planner entity lookup returned {StatusCode}.", response.StatusCode);
                return new DistrictDatasetResult();
            }

            var body = await response.Content.ReadFromJsonAsync<EntitiesResponse>(cancellationToken: cancellationToken);
            if (body is null || !body.Found)
            {
                return new DistrictDatasetResult();
            }

            return new DistrictDatasetResult
            {
                Found = true,
                UnmatchedInterests = body.UnmatchedInterests ?? [],
                FallbackDistricts = body.FallbackDistricts ?? [],
                Places = (body.Entities ?? []).Select(e => new DatasetPlaceDto
                {
                    Id = StableId(e.Key),
                    Key = e.Key,
                    Name = e.Name,
                    Area = e.Area,
                    EntityType = e.EntityType,
                    Interests = e.Interests ?? [],
                    MatchedInterests = e.MatchedInterests ?? [],
                    Description = e.Description,
                }).ToList(),
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(exc, "Travel Planner entity lookup failed.");
            return new DistrictDatasetResult();
        }
    }

    // Same key -> same Guid on every request, so a saved plan's stop keeps its identity.
    private static Guid StableId(string key)
        => new(System.Security.Cryptography.MD5.HashData(System.Text.Encoding.UTF8.GetBytes("dataset-place:" + key)));

    private record EntitiesRequest(
        [property: JsonPropertyName("district")] string District,
        [property: JsonPropertyName("interests")] List<string>? Interests,
        [property: JsonPropertyName("limit")] int Limit);

    private class EntitiesResponse
    {
        [JsonPropertyName("found")] public bool Found { get; set; }
        [JsonPropertyName("entities")] public List<EntityItem>? Entities { get; set; }
        [JsonPropertyName("unmatchedInterests")] public List<string>? UnmatchedInterests { get; set; }
        [JsonPropertyName("fallbackDistricts")] public List<string>? FallbackDistricts { get; set; }
    }

    private class EntityItem
    {
        [JsonPropertyName("key")] public string Key { get; set; } = string.Empty;
        [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
        [JsonPropertyName("area")] public string? Area { get; set; }
        [JsonPropertyName("entityType")] public string? EntityType { get; set; }
        [JsonPropertyName("interests")] public List<string>? Interests { get; set; }
        [JsonPropertyName("matchedInterests")] public List<string>? MatchedInterests { get; set; }
        [JsonPropertyName("description")] public string? Description { get; set; }
    }

    private record RetrieveRequest(
        [property: JsonPropertyName("query")] string Query,
        [property: JsonPropertyName("district")] string? District,
        [property: JsonPropertyName("interests")] List<string>? Interests);

    private class RetrieveResponse
    {
        [JsonPropertyName("results")]
        public List<RetrievedSnippet>? Results { get; set; }
    }

    private class RetrievedSnippet
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("placeType")]
        public string? PlaceType { get; set; }
    }
}
