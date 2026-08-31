using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/field-research/surveys/{surveyId:guid}/responses")]
public class SurveyResponsesController : ControllerBase
{
    private readonly ISurveyResponseService _responseService;

    public SurveyResponsesController(ISurveyResponseService responseService)
    {
        _responseService = responseService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<SurveyResponseListItemDto>>> GetForSurvey(
        Guid surveyId, [FromQuery] SurveyResponseQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _responseService.GetForSurveyAsync(CurrentUserId, surveyId, query, cancellationToken));

    [HttpGet("{responseId:guid}")]
    public async Task<ActionResult<SurveyResponseDto>> GetById(
        Guid surveyId, Guid responseId, CancellationToken cancellationToken)
        => Ok(await _responseService.GetByIdAsync(CurrentUserId, surveyId, responseId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<SurveyResponseDto>> Create(
        Guid surveyId, CreateSurveyResponseRequest request, CancellationToken cancellationToken)
    {
        var result = await _responseService.CreateAsync(CurrentUserId, surveyId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { surveyId, responseId = result.Id }, result);
    }

    [HttpPut("{responseId:guid}")]
    public async Task<ActionResult<SurveyResponseDto>> Update(
        Guid surveyId, Guid responseId, UpdateSurveyResponseRequest request, CancellationToken cancellationToken)
        => Ok(await _responseService.UpdateAsync(CurrentUserId, surveyId, responseId, request, cancellationToken));

    [HttpPost("{responseId:guid}/submit")]
    public async Task<ActionResult<SurveyResponseDto>> Submit(
        Guid surveyId, Guid responseId, CancellationToken cancellationToken)
        => Ok(await _responseService.SubmitAsync(CurrentUserId, surveyId, responseId, cancellationToken));

    [HttpPost("{responseId:guid}/review")]
    public async Task<ActionResult<SurveyResponseDto>> Review(
        Guid surveyId, Guid responseId, ReviewSurveyResponseRequest request, CancellationToken cancellationToken)
        => Ok(await _responseService.ReviewAsync(CurrentUserId, surveyId, responseId, request, cancellationToken));

    [HttpDelete("{responseId:guid}")]
    public async Task<IActionResult> Delete(Guid surveyId, Guid responseId, CancellationToken cancellationToken)
    {
        await _responseService.DeleteAsync(CurrentUserId, surveyId, responseId, cancellationToken);
        return NoContent();
    }
}
