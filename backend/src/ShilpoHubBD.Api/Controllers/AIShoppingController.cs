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

    [HttpPost("interior-preview")]
    public async Task<ActionResult<InteriorPreviewDto>> GetInteriorPreview(InteriorPreviewRequest request, CancellationToken cancellationToken)
    {
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
