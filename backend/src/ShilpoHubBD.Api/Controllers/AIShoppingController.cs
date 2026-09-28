using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/ai-shopping")]
public class AIShoppingController : ControllerBase
{
    private readonly IGiftRecommendationService _giftRecommendationService;
    private readonly IFashionMatchingService _fashionMatchingService;
    private readonly IInteriorPreviewService _interiorPreviewService;
    private readonly ITranslationService _translationService;

    public AIShoppingController(
        IGiftRecommendationService giftRecommendationService,
        IFashionMatchingService fashionMatchingService,
        IInteriorPreviewService interiorPreviewService,
        ITranslationService translationService)
    {
        _giftRecommendationService = giftRecommendationService;
        _fashionMatchingService = fashionMatchingService;
        _interiorPreviewService = interiorPreviewService;
        _translationService = translationService;
    }

    [HttpPost("gift-recommendations")]
    public async Task<ActionResult<List<GiftSuggestionDto>>> GetGiftRecommendations(GiftRecommendationRequest request, CancellationToken cancellationToken)
    {
        var result = await _giftRecommendationService.GetSuggestionsAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("fashion-matches")]
    public async Task<ActionResult<List<FashionMatchDto>>> GetFashionMatches(FashionMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _fashionMatchingService.GetMatchesAsync(request, cancellationToken);
        return Ok(result);
    }

    // multipart/form-data, same upload convention as MediaController/ChatMediaController/ProfileController:
    // a real room photo plus the product to place in it, not a JSON body of free-text fields.
    [HttpPost("interior-preview")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 20 * 1024 * 1024)]
    public async Task<ActionResult<InteriorPreviewDto>> GetInteriorPreview(
        [FromForm] Guid productId, IFormFile roomImage, [FromForm] string? style, CancellationToken cancellationToken)
    {
        var roomImageBytes = Array.Empty<byte>();
        if (roomImage is not null)
        {
            await using var stream = new MemoryStream();
            await roomImage.CopyToAsync(stream, cancellationToken);
            roomImageBytes = stream.ToArray();
        }

        var request = new InteriorPreviewRequest
        {
            ProductId = productId,
            RoomImageBytes = roomImageBytes,
            RoomImageContentType = roomImage?.ContentType ?? string.Empty,
            Style = style,
        };

        var result = await _interiorPreviewService.GetPreviewAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("translate")]
    public async Task<ActionResult<TranslationResultDto>> Translate(TranslationRequest request, CancellationToken cancellationToken)
    {
        var result = await _translationService.TranslateAsync(request, cancellationToken);
        return Ok(result);
    }
}
