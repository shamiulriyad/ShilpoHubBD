using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Infrastructure.Options;

namespace ShilpoHubBD.Infrastructure.AIShopping;

/// <summary>
/// Real interior preview: composites an actual, database-verified product photo into the shopper's own
/// uploaded room photo via a Gemini image-editing model. There is no honest placeholder for "here is
/// your room with this product in it" (unlike a rule-based estimate for a number), so an invalid
/// product/image, missing config, upstream failure or storage failure all throw a clear exception
/// instead of ever returning a static or unrelated image.
///
/// NOTE: the Gemini request/response shape below (multiple inlineData image parts in, one inlineData
/// image part out) follows the documented image-editing generateContent schema at the time this was
/// written. Verify it against a live call with the configured ImageModel before relying on it in
/// production -- image-editing API shapes are more likely to have shifted than the plain-text shape
/// used elsewhere in this codebase.
/// </summary>
public class GeminiInteriorPreviewProvider : IInteriorPreviewService
{
    private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg", "image/png", "image/webp",
    };

    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _geminiOptions;
    private readonly ImageStorageOptions _imageStorageOptions;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<GeminiInteriorPreviewProvider> _logger;

    public GeminiInteriorPreviewProvider(
        HttpClient httpClient,
        IOptions<GeminiOptions> geminiOptions,
        IOptions<ImageStorageOptions> imageStorageOptions,
        IProductRepository productRepository,
        ILogger<GeminiInteriorPreviewProvider> logger)
    {
        _httpClient = httpClient;
        _geminiOptions = geminiOptions.Value;
        _imageStorageOptions = imageStorageOptions.Value;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<InteriorPreviewDto> GetPreviewAsync(InteriorPreviewRequest request, CancellationToken cancellationToken)
    {
        ValidateRoomImage(request);

        if (string.IsNullOrWhiteSpace(_geminiOptions.ApiKey)
            || string.IsNullOrWhiteSpace(_geminiOptions.ImageModel)
            || string.IsNullOrWhiteSpace(_imageStorageOptions.WebRootPath))
        {
            _logger.LogWarning("Interior preview requested but Gemini image generation is not configured.");
            throw new AiServiceUnavailableException("Interior preview is temporarily unavailable: the service is not configured.");
        }

        var product = await _productRepository.GetByIdAsync(request.ProductId, cancellationToken)
            ?? throw new NotFoundException($"Product '{request.ProductId}' was not found.");

        var productImageUrl = product.Images
            .OrderByDescending(i => i.IsPrimary)
            .ThenBy(i => i.DisplayOrder)
            .Select(i => i.ImageUrl)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(productImageUrl))
        {
            throw new ConflictException($"'{product.Name}' has no product image to preview.");
        }

        var (productImageBytes, productMimeType) = await LoadStoredImageAsync(productImageUrl, cancellationToken);

        _logger.LogInformation(
            "Interior preview requested. ProductId={ProductId} RoomImageBytes={RoomImageBytes}",
            request.ProductId, request.RoomImageBytes.Length);

        byte[] resultBytes;
        string resultMimeType;
        try
        {
            var prompt = BuildPrompt(request.Style);
            (resultBytes, resultMimeType) = await CallGeminiImageAsync(
                prompt, request.RoomImageBytes, request.RoomImageContentType, productImageBytes, productMimeType, cancellationToken);
        }
        catch (Exception exc) when (exc is HttpRequestException or TaskCanceledException or JsonException or FormatException)
        {
            _logger.LogWarning(exc, "Gemini interior-preview compositing failed. ProductId={ProductId} FailureType={FailureType}",
                request.ProductId, exc.GetType().Name);
            throw new AiServiceUnavailableException("Interior preview is temporarily unavailable. Please try again shortly.");
        }

        string fileName;
        try
        {
            var extension = resultMimeType.Contains("png", StringComparison.OrdinalIgnoreCase) ? ".png" : ".jpg";
            var folder = Path.Combine(_imageStorageOptions.WebRootPath, "uploads", "images");
            Directory.CreateDirectory(folder);
            fileName = $"{Guid.NewGuid():N}{extension}";
            await File.WriteAllBytesAsync(Path.Combine(folder, fileName), resultBytes, cancellationToken);
        }
        catch (Exception exc) when (exc is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            _logger.LogError(exc, "Failed to save the generated interior preview. ProductId={ProductId}", request.ProductId);
            throw new AiServiceUnavailableException("The preview was generated but could not be saved. Please try again.");
        }

        _logger.LogInformation("Interior preview completed. ProductId={ProductId}", request.ProductId);

        return new InteriorPreviewDto
        {
            PreviewImageUrl = $"/uploads/images/{fileName}",
            Description = $"AI-generated preview: {product.Name} placed in your room.",
        };
    }

    private static void ValidateRoomImage(InteriorPreviewRequest request)
    {
        var failures = new List<FluentValidation.Results.ValidationFailure>();
        if (request.RoomImageBytes.Length == 0)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(nameof(request.RoomImageBytes), "A room photo is required."));
        }
        else if (request.RoomImageBytes.Length > 20 * 1024 * 1024)
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(nameof(request.RoomImageBytes), "The room photo must be smaller than 20 MB."));
        }

        if (request.RoomImageBytes.Length > 0 && !AllowedImageTypes.Contains(request.RoomImageContentType))
        {
            failures.Add(new FluentValidation.Results.ValidationFailure(
                nameof(request.RoomImageContentType), "Only JPG, PNG and WebP room photos are supported."));
        }

        if (failures.Count > 0)
        {
            throw new FluentValidation.ValidationException(failures);
        }
    }

    private async Task<(byte[] Bytes, string MimeType)> LoadStoredImageAsync(string imageUrl, CancellationToken cancellationToken)
    {
        var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_imageStorageOptions.WebRootPath, relativePath);

        try
        {
            var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);
            return (bytes, MimeTypeFromExtension(fullPath));
        }
        catch (Exception exc) when (exc is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            _logger.LogError(exc, "The stored product image could not be read from disk. Path={Path}", fullPath);
            throw new AiServiceUnavailableException("The product image could not be loaded. Please try again shortly.");
        }
    }

    private static string MimeTypeFromExtension(string path) => Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".png" => "image/png",
        ".webp" => "image/webp",
        _ => "image/jpeg",
    };

    private static string BuildPrompt(string? style)
    {
        var sb = new StringBuilder();
        sb.AppendLine("You are compositing a real product photo (the second image) into a shopper's real room photo (the first image).");
        sb.AppendLine("Rules:");
        sb.AppendLine("- Preserve the room's layout, furniture, walls and lighting exactly as shown in the first image.");
        sb.AppendLine("- Place the product from the second image naturally into the room, as if it were physically present.");
        sb.AppendLine("- Keep the product's proportions in approximately correct scale and perspective relative to the room.");
        sb.AppendLine("- Preserve the product's real shape, color, pattern and material exactly as shown in the second image -- do not redesign, restyle or reinterpret it.");
        if (!string.IsNullOrWhiteSpace(style))
        {
            sb.AppendLine($"- Placement guidance from the shopper: {style.Trim()}.");
        }

        sb.AppendLine("Output only the final composited photo. No text, labels or watermark.");
        return sb.ToString();
    }

    private async Task<(byte[] Bytes, string MimeType)> CallGeminiImageAsync(
        string prompt, byte[] roomImage, string roomMimeType, byte[] productImage, string productMimeType, CancellationToken cancellationToken)
    {
        var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"models/{_geminiOptions.ImageModel}:generateContent")
        {
            Content = new StringContent(JsonSerializer.Serialize(new GeminiRequest
            {
                Contents =
                [
                    new GeminiContent
                    {
                        Parts =
                        [
                            new GeminiPart { Text = prompt },
                            new GeminiPart { InlineData = new GeminiInlineData { MimeType = roomMimeType, Data = Convert.ToBase64String(roomImage) } },
                            new GeminiPart { InlineData = new GeminiInlineData { MimeType = productMimeType, Data = Convert.ToBase64String(productImage) } },
                        ],
                    },
                ],
            }), Encoding.UTF8, "application/json"),
        };
        httpRequest.Headers.Add("x-goog-api-key", _geminiOptions.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: cancellationToken);
        var imagePart = body?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault(p => p.InlineData is not null);
        if (imagePart?.InlineData?.Data is not { Length: > 0 } data)
        {
            throw new JsonException("Gemini response contained no image data.");
        }

        return (Convert.FromBase64String(data), imagePart.InlineData.MimeType ?? "image/png");
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
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Text { get; set; }

        [JsonPropertyName("inlineData")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public GeminiInlineData? InlineData { get; set; }
    }

    private class GeminiInlineData
    {
        [JsonPropertyName("mimeType")]
        public string? MimeType { get; set; }

        [JsonPropertyName("data")]
        public string? Data { get; set; }
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
