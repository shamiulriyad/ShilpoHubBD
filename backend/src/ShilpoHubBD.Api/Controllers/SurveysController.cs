using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/field-research/surveys")]
public class SurveysController : ControllerBase
{
    private readonly ISurveyService _surveyService;

    public SurveysController(ISurveyService surveyService)
    {
        _surveyService = surveyService;
    }

    internal const string ResearcherRoles =
        $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}";

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<SurveyListItemDto>>> GetMine(
        [FromQuery] SurveyQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _surveyService.GetForUserAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SurveyDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _surveyService.GetByIdAsync(CurrentUserId, id, cancellationToken));

    [Authorize(Roles = ResearcherRoles)]
    [HttpPost]
    public async Task<ActionResult<SurveyDetailDto>> Create(CreateSurveyRequest request, CancellationToken cancellationToken)
    {
        var result = await _surveyService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SurveyDetailDto>> Update(Guid id, UpdateSurveyRequest request, CancellationToken cancellationToken)
        => Ok(await _surveyService.UpdateAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/status")]
    public async Task<ActionResult<SurveyDetailDto>> UpdateStatus(
        Guid id, UpdateSurveyStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _surveyService.UpdateStatusAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _surveyService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/questions")]
    public async Task<ActionResult<SurveyQuestionDto>> AddQuestion(
        Guid id, CreateSurveyQuestionRequest request, CancellationToken cancellationToken)
        => Ok(await _surveyService.AddQuestionAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/questions/{questionId:guid}")]
    public async Task<ActionResult<SurveyQuestionDto>> UpdateQuestion(
        Guid id, Guid questionId, UpdateSurveyQuestionRequest request, CancellationToken cancellationToken)
        => Ok(await _surveyService.UpdateQuestionAsync(CurrentUserId, id, questionId, request, cancellationToken));

    [HttpDelete("{id:guid}/questions/{questionId:guid}")]
    public async Task<IActionResult> DeleteQuestion(Guid id, Guid questionId, CancellationToken cancellationToken)
    {
        await _surveyService.DeleteQuestionAsync(CurrentUserId, id, questionId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/field-researchers")]
    public async Task<ActionResult<List<SurveyFieldAssignmentDto>>> GetFieldResearchers(Guid id, CancellationToken cancellationToken)
        => Ok(await _surveyService.GetFieldResearchersAsync(CurrentUserId, id, cancellationToken));

    [HttpPost("{id:guid}/field-researchers")]
    public async Task<ActionResult<SurveyFieldAssignmentDto>> AssignFieldResearcher(
        Guid id, AssignFieldResearcherRequest request, CancellationToken cancellationToken)
        => Ok(await _surveyService.AssignFieldResearcherAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/field-researchers/{assignmentId:guid}")]
    public async Task<ActionResult<SurveyFieldAssignmentDto>> UpdateFieldAssignment(
        Guid id, Guid assignmentId, UpdateFieldAssignmentRequest request, CancellationToken cancellationToken)
        => Ok(await _surveyService.UpdateFieldAssignmentAsync(CurrentUserId, id, assignmentId, request, cancellationToken));

    [HttpDelete("{id:guid}/field-researchers/{assignmentId:guid}")]
    public async Task<IActionResult> RemoveFieldResearcher(Guid id, Guid assignmentId, CancellationToken cancellationToken)
    {
        await _surveyService.RemoveFieldResearcherAsync(CurrentUserId, id, assignmentId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/history")]
    public async Task<ActionResult<List<DataCollectionEventDto>>> GetHistory(
        Guid id, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
        => Ok(await _surveyService.GetHistoryAsync(CurrentUserId, id, take, cancellationToken));
}
