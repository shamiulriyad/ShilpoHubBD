using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/innovation-lab/prototypes")]
public class InnovationPrototypesController : ControllerBase
{
    private readonly IInnovationPrototypeService _prototypeService;

    public InnovationPrototypesController(IInnovationPrototypeService prototypeService)
    {
        _prototypeService = prototypeService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsResearcher =>
        User.IsInRole(RoleNames.HeritageInnovationHub) || User.IsInRole(RoleNames.GovernmentNGO)
        || User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<InnovationPrototypeListItemDto>>> GetMine(
        [FromQuery] InnovationPrototypeQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _prototypeService.GetMineAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InnovationPrototypeDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _prototypeService.GetByIdAsync(CurrentUserId, id, cancellationToken));

    [Authorize(Roles = InnovationExperimentsController.ResearcherRoles)]
    [HttpPost]
    public async Task<ActionResult<InnovationPrototypeDetailDto>> Create(
        CreateInnovationPrototypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _prototypeService.CreateAsync(CurrentUserId, IsResearcher, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<InnovationPrototypeDetailDto>> Update(
        Guid id, UpdateInnovationPrototypeRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.UpdateAsync(CurrentUserId, IsResearcher, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _prototypeService.DeleteAsync(CurrentUserId, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/iterations")]
    public async Task<ActionResult<PrototypeIterationDto>> AddIteration(
        Guid id, CreatePrototypeIterationRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.AddIterationAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/test-cases")]
    public async Task<ActionResult<PrototypeTestCaseDto>> AddTestCase(
        Guid id, CreatePrototypeTestCaseRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.AddTestCaseAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/test-cases/{testCaseId:guid}")]
    public async Task<ActionResult<PrototypeTestCaseDto>> UpdateTestCase(
        Guid id, Guid testCaseId, UpdatePrototypeTestCaseRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.UpdateTestCaseAsync(CurrentUserId, id, testCaseId, request, cancellationToken));

    [HttpDelete("{id:guid}/test-cases/{testCaseId:guid}")]
    public async Task<IActionResult> DeleteTestCase(Guid id, Guid testCaseId, CancellationToken cancellationToken)
    {
        await _prototypeService.DeleteTestCaseAsync(CurrentUserId, id, testCaseId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/test-runs")]
    public async Task<ActionResult<PagedResult<PrototypeTestRunDto>>> GetTestRuns(
        Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _prototypeService.GetTestRunsAsync(CurrentUserId, id, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}/test-runs/{testRunId:guid}")]
    public async Task<ActionResult<PrototypeTestRunDto>> GetTestRun(
        Guid id, Guid testRunId, CancellationToken cancellationToken)
        => Ok(await _prototypeService.GetTestRunAsync(CurrentUserId, id, testRunId, cancellationToken));

    [HttpPost("{id:guid}/test-runs")]
    public async Task<ActionResult<PrototypeTestRunDto>> CreateTestRun(
        Guid id, CreatePrototypeTestRunRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.CreateTestRunAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/test-runs/{testRunId:guid}")]
    public async Task<ActionResult<PrototypeTestRunDto>> UpdateTestRun(
        Guid id, Guid testRunId, UpdatePrototypeTestRunRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.UpdateTestRunAsync(CurrentUserId, id, testRunId, request, cancellationToken));

    [HttpDelete("{id:guid}/test-runs/{testRunId:guid}")]
    public async Task<IActionResult> DeleteTestRun(Guid id, Guid testRunId, CancellationToken cancellationToken)
    {
        await _prototypeService.DeleteTestRunAsync(CurrentUserId, id, testRunId, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/issues")]
    public async Task<ActionResult<PagedResult<PrototypeIssueDto>>> GetIssues(
        Guid id, [FromQuery] string? status = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _prototypeService.GetIssuesAsync(CurrentUserId, id, status, page, pageSize, cancellationToken));

    [HttpPost("{id:guid}/issues")]
    public async Task<ActionResult<PrototypeIssueDto>> CreateIssue(
        Guid id, CreatePrototypeIssueRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.CreateIssueAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPut("{id:guid}/issues/{issueId:guid}")]
    public async Task<ActionResult<PrototypeIssueDto>> UpdateIssue(
        Guid id, Guid issueId, UpdatePrototypeIssueRequest request, CancellationToken cancellationToken)
        => Ok(await _prototypeService.UpdateIssueAsync(CurrentUserId, id, issueId, request, cancellationToken));

    [HttpDelete("{id:guid}/issues/{issueId:guid}")]
    public async Task<IActionResult> DeleteIssue(Guid id, Guid issueId, CancellationToken cancellationToken)
    {
        await _prototypeService.DeleteIssueAsync(CurrentUserId, id, issueId, cancellationToken);
        return NoContent();
    }
}
