using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.ProductSearch;

/// <summary>
/// Asks the Product Search service (Gemini) to SUGGEST attributes for a product from its own text. The service returns
/// values validated against the marketplace vocabulary; the caller stores them as a pending suggestion that the producer
/// must confirm. Any failure returns null.
/// </summary>
public class PythonAttributeSuggester : IProductAttributeSuggester
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly ILogger<PythonAttributeSuggester> _logger;

    public PythonAttributeSuggester(HttpClient http, IConfiguration configuration, IOptions<ProductSearchServiceOptions> options, ILogger<PythonAttributeSuggester> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(Math.Max(options.Value.TimeoutSeconds, 40));   // an LLM call, slower than a search
        _apiKey = configuration["ProductIndex:ApiKey"];
        _logger = logger;
    }

    public async Task<GeneratedAttributeSuggestion?> SuggestAsync(ProductForSuggestion product, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/product-search/suggest-attributes")
            {
                Content = JsonContent.Create(new { name = product.Name, description = product.Description, story = product.Story, category = product.Category, district = product.District }),
            };
            request.Headers.Add("X-Internal-Key", _apiKey);
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Attribute suggestion service returned {Status}.", (int)response.StatusCode);
                return null;
            }

            var body = await response.Content.ReadFromJsonAsync<SuggesterResponse>(cancellationToken: cancellationToken);
            return body is null ? null : new GeneratedAttributeSuggestion { Suggested = body.Suggested, Model = body.Model };
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Attribute suggestion service unavailable.");
            return null;
        }
    }

    private sealed class SuggesterResponse
    {
        [JsonPropertyName("suggested")] public System.Text.Json.JsonElement Suggested { get; set; }
        [JsonPropertyName("model")] public string Model { get; set; } = string.Empty;
    }
}
