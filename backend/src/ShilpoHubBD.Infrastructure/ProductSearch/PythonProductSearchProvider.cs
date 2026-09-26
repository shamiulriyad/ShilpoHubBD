using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Infrastructure.ProductSearch;

public class ProductSearchServiceOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8001";
    public int TimeoutSeconds { get; set; } = 25;
}

/// <summary>
/// Talks to the separate Product Search service (rag/product_main.py). Deliberately not the Heritage RAG
/// (<c>RagService</c>) or the Travel Planner: product search has its own service, collection and settings.
/// Any failure returns null so the caller falls back to keyword search instead of failing the request.
/// </summary>
public class PythonProductSearchProvider : IProductSearchCandidateProvider
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly ILogger<PythonProductSearchProvider> _logger;

    public PythonProductSearchProvider(HttpClient http, IConfiguration configuration, IOptions<ProductSearchServiceOptions> options, ILogger<PythonProductSearchProvider> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
        _apiKey = configuration["ProductIndex:ApiKey"];   // the same shared secret the index feed uses
        _logger = logger;
    }

    public async Task<ProductSearchCandidatesDto?> GetCandidatesAsync(string query, int limit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/product-search/candidates") { Content = JsonContent.Create(new { query, limit }) };
            request.Headers.Add("X-Internal-Key", _apiKey);
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Product search service returned {Status}; using keyword fallback.", (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<ProductSearchCandidatesDto>(cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Product search service unavailable; using keyword fallback.");
            return null;
        }
    }
}
