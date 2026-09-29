using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.ProductSearch;

namespace ShilpoHubBD.Infrastructure.ReviewSimilarity;

/// <summary>
/// Talks to the SAME Python RAG service as product search (rag/product_main.py, reusing
/// <see cref="ProductSearchServiceOptions"/> — no new service, no new config section) for review similarity, but
/// its own endpoint and its own review vector collection on that side, isolated from product search. Any
/// failure returns null so the caller falls back to a plain database query instead of failing.
/// </summary>
public class PythonReviewSimilarityProvider : IReviewSimilarityProvider
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly ILogger<PythonReviewSimilarityProvider> _logger;

    public PythonReviewSimilarityProvider(HttpClient http, IConfiguration configuration, IOptions<ProductSearchServiceOptions> options, ILogger<PythonReviewSimilarityProvider> logger)
    {
        _http = http;
        _http.BaseAddress = new Uri(options.Value.BaseUrl);
        _http.Timeout = TimeSpan.FromSeconds(options.Value.TimeoutSeconds);
        _apiKey = configuration["ProductIndex:ApiKey"];   // the same shared secret the index feed uses
        _logger = logger;
    }

    public async Task<List<SimilarReviewMatchDto>?> FindSimilarAsync(Guid productId, string queryText, int limit, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_apiKey) || string.IsNullOrWhiteSpace(queryText))
        {
            return null;
        }

        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "api/review-search/similar")
            {
                Content = JsonContent.Create(new { productId, query = queryText, limit }),
            };
            request.Headers.Add("X-Internal-Key", _apiKey);
            using var response = await _http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Review similarity service returned {Status}; using the database fallback.", (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<List<SimilarReviewMatchDto>>(cancellationToken: cancellationToken);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
        {
            _logger.LogWarning(ex, "Review similarity service unavailable; using the database fallback.");
            return null;
        }
    }
}
