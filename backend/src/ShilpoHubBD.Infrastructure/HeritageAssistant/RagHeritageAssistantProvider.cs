using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.HeritageAssistant;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.HeritageAssistant;

/// <summary>Calls the ShilpoHub RAG service (FastAPI, `rag/main.py`) over HTTP for real,
/// dataset-grounded answers. Falls back to a friendly "unavailable" answer — never throws — so a
/// down RAG service degrades the assistant instead of breaking the request.</summary>
public class RagHeritageAssistantProvider : IHeritageAssistantProvider
{
    private readonly HttpClient _httpClient;
    private readonly RagServiceOptions _options;
    private readonly ILogger<RagHeritageAssistantProvider> _logger;

    public RagHeritageAssistantProvider(HttpClient httpClient, IOptions<RagServiceOptions> options, ILogger<RagHeritageAssistantProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<HeritageAssistantAnswerDto> AnswerAsync(HeritageAssistantContext context, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/kb/{Uri.EscapeDataString(_options.Collection)}/query",
                new RagQueryRequest(context.Question),
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "RAG service returned {StatusCode} for a heritage assistant question.", response.StatusCode);
                return Unavailable();
            }

            var result = await response.Content.ReadFromJsonAsync<RagQueryResponse>(cancellationToken);
            if (result is null)
            {
                return Unavailable();
            }

            return new HeritageAssistantAnswerDto
            {
                Answer = result.Answer,
                Category = result.QuestionType,
                Sources = (result.Sources ?? [])
                    .Select(s => s.SourceFile)
                    .Where(file => !string.IsNullOrWhiteSpace(file))
                    .Distinct()
                    .ToList(),
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(exc, "RAG service call failed for a heritage assistant question.");
            return Unavailable();
        }
    }

    private static HeritageAssistantAnswerDto Unavailable() => new()
    {
        Answer = "The Heritage AI is temporarily unavailable. Please try again in a moment.",
    };

    private record RagQueryRequest([property: JsonPropertyName("question")] string Question);

    private class RagQueryResponse
    {
        [JsonPropertyName("answer")]
        public string Answer { get; set; } = string.Empty;

        [JsonPropertyName("question_type")]
        public string QuestionType { get; set; } = string.Empty;

        [JsonPropertyName("sources")]
        public List<RagSource>? Sources { get; set; }
    }

    private class RagSource
    {
        [JsonPropertyName("source_file")]
        public string SourceFile { get; set; } = string.Empty;
    }
}
