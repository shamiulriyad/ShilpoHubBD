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
[Route("api/field-research/surveys/{surveyId:guid}/evidence")]
public class FieldEvidenceController : ControllerBase
{
    private readonly IFieldEvidenceService _evidenceService;

    public FieldEvidenceController(IFieldEvidenceService evidenceService)
    {
        _evidenceService = evidenceService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<FieldEvidenceDto>>> GetForSurvey(
        Guid surveyId, [FromQuery] FieldEvidenceQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _evidenceService.GetForSurveyAsync(CurrentUserId, surveyId, query, cancellationToken));

    [HttpGet("{evidenceId:guid}")]
    public async Task<ActionResult<FieldEvidenceDto>> GetById(
        Guid surveyId, Guid evidenceId, CancellationToken cancellationToken)
        => Ok(await _evidenceService.GetByIdAsync(CurrentUserId, surveyId, evidenceId, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<FieldEvidenceDto>> Create(
        Guid surveyId, CreateFieldEvidenceRequest request, CancellationToken cancellationToken)
    {
        var result = await _evidenceService.CreateAsync(CurrentUserId, surveyId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { surveyId, evidenceId = result.Id }, result);
    }

    [HttpPut("{evidenceId:guid}")]
    public async Task<ActionResult<FieldEvidenceDto>> Update(
        Guid surveyId, Guid evidenceId, UpdateFieldEvidenceRequest request, CancellationToken cancellationToken)
        => Ok(await _evidenceService.UpdateAsync(CurrentUserId, surveyId, evidenceId, request, cancellationToken));

    [HttpDelete("{evidenceId:guid}")]
    public async Task<IActionResult> Delete(Guid surveyId, Guid evidenceId, CancellationToken cancellationToken)
    {
        await _evidenceService.DeleteAsync(CurrentUserId, surveyId, evidenceId, cancellationToken);
        return NoContent();
    }
}
