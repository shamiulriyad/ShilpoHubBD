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
