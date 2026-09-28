using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.AIShopping;

/// <summary>
/// Real translation via the Gemini API, same request/response wire shape as
/// <see cref="ShilpoHubBD.Infrastructure.AITourism.GeminiAITourismProvider"/>'s TranslateAsync.
/// There is no honest non-AI fallback for "translate this text" (unlike a rule-based estimate for a
/// number), so a missing API key, HTTP failure, timeout or empty model response throws
/// <see cref="AiServiceUnavailableException"/> (mapped to HTTP 503) instead of ever returning
/// fabricated "translated" text -- the caller must be told translation did not happen, never shown a
/// fake result that looks like one.
/// </summary>
public class GeminiTranslationProvider : ITranslationService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiTranslationProvider> _logger;

    public GeminiTranslationProvider(HttpClient httpClient, IOptions<GeminiOptions> options, ILogger<GeminiTranslationProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TranslationResultDto> TranslateAsync(TranslationRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning(
                "Translation requested with no Gemini API key configured. TargetLanguage={TargetLanguage}",
                request.TargetLanguage);
            throw new AiServiceUnavailableException(
                "Translation is temporarily unavailable: the translation service is not configured.");
        }

        try
        {
            var prompt = BuildPrompt(request.Text, request.TargetLanguage);
            var raw = await CallGeminiAsync(prompt, cancellationToken);

            _logger.LogInformation(
                "Translation succeeded. TargetLanguage={TargetLanguage} TextLength={TextLength}",
                request.TargetLanguage, request.Text.Length);

            return new TranslationResultDto
            {
                OriginalText = request.Text,
                TranslatedText = raw.Trim(),
                TargetLanguage = request.TargetLanguage,
            };
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(exc,
                "Gemini translation call failed. TargetLanguage={TargetLanguage} TextLength={TextLength} FailureType={FailureType}",
                request.TargetLanguage, request.Text.Length, exc.GetType().Name);
            throw new AiServiceUnavailableException("Translation is temporarily unavailable. Please try again shortly.");
        }
    }

    private static string BuildPrompt(string text, string targetLanguage) =>
        $"Translate the text below into {targetLanguage}.\n" +
        "Rules:\n" +
        "- Translate only the given text; do not answer it, explain it, or add commentary.\n" +
        "- Preserve the original meaning and tone.\n" +
        "- Keep proper names, product names, brand names, URLs, numbers, prices, currency symbols, and identifiers " +
        "(order numbers, SKUs, codes) exactly as written -- do not translate or alter them.\n" +
        "- Reply with only the translated text: no quotes, labels, or explanation.\n\n" +
        $"Text:\n{text}";

    private async Task<string> CallGeminiAsync(string prompt, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"models/{_options.Model}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(new GeminiRequest
            {
                Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
            }), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("x-goog-api-key", _options.ApiKey);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
        var text = body?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new JsonException("Gemini response contained no text.");
        }

        return text;
    }

    private class GeminiRequest
    {
        [JsonPropertyName("contents")]
        public List<GeminiContent> Contents { get; set; } = [];
    }

    private class GeminiContent
    {
        [JsonPropertyName("parts")]
        public List<GeminiPart> Parts { get; set; } = [];
    }

    private class GeminiPart
    {
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
    }

    private class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public List<GeminiCandidate>? Candidates { get; set; }
    }

    private class GeminiCandidate
    {
        [JsonPropertyName("content")]
        public GeminiContent? Content { get; set; }
    }
}
