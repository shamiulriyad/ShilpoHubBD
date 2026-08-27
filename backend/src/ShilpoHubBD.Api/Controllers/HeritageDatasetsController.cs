using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/heritage-database/datasets")]
public class HeritageDatasetsController : ControllerBase
{
    private readonly IHeritageDatasetService _datasetService;

    public HeritageDatasetsController(IHeritageDatasetService datasetService)
    {
        _datasetService = datasetService;
    }

    internal const string ResearcherRoles =
        $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}";

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private HeritageDbAccessContext AccessContext => new()
    {
        UserId = CurrentUserId,
        IsSuperAdmin = User.IsInRole(RoleNames.SuperAdmin),
        IsResearcher = User.IsInRole(RoleNames.HeritageInnovationHub)
            || User.IsInRole(RoleNames.GovernmentNGO)
            || User.IsInRole(RoleNames.SuperAdmin),
    };

    [HttpGet]
    public async Task<ActionResult<PagedResult<HeritageDatasetListItemDto>>> GetAccessible(
        [FromQuery] HeritageDatasetQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetAccessibleAsync(AccessContext, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<HeritageDatasetDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetByIdAsync(AccessContext, id, cancellationToken));

    [Authorize(Roles = ResearcherRoles)]
    [HttpPost]
    public async Task<ActionResult<HeritageDatasetDetailDto>> Create(
        CreateHeritageDatasetRequest request, CancellationToken cancellationToken)
    {
        var result = await _datasetService.CreateAsync(AccessContext, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<HeritageDatasetDetailDto>> Update(
        Guid id, UpdateHeritageDatasetRequest request, CancellationToken cancellationToken)
        => Ok(await _datasetService.UpdateAsync(AccessContext, id, request, cancellationToken));

    [HttpPost("{id:guid}/refresh")]
    public async Task<ActionResult<HeritageDatasetDetailDto>> Refresh(Guid id, CancellationToken cancellationToken)
        => Ok(await _datasetService.RefreshAsync(AccessContext, id, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _datasetService.DeleteAsync(AccessContext, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<ActionResult<List<HeritageDatasetVersionDto>>> GetVersions(Guid id, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetVersionsAsync(AccessContext, id, cancellationToken));

    [HttpPost("{id:guid}/versions")]
    public async Task<ActionResult<HeritageDatasetVersionDto>> AddVersion(
        Guid id, CreateHeritageDatasetVersionRequest request, CancellationToken cancellationToken)
        => Ok(await _datasetService.AddVersionAsync(AccessContext, id, request, cancellationToken));

    [HttpGet("{id:guid}/access-grants")]
    public async Task<ActionResult<List<HeritageDatasetAccessGrantDto>>> GetAccessGrants(Guid id, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetAccessGrantsAsync(AccessContext, id, cancellationToken));

    [HttpPost("{id:guid}/access-grants")]
    public async Task<ActionResult<HeritageDatasetAccessGrantDto>> GrantAccess(
        Guid id, GrantHeritageDatasetAccessRequest request, CancellationToken cancellationToken)
        => Ok(await _datasetService.GrantAccessAsync(AccessContext, id, request, cancellationToken));

    [HttpDelete("{id:guid}/access-grants/{grantId:guid}")]
    public async Task<IActionResult> RevokeAccess(Guid id, Guid grantId, CancellationToken cancellationToken)
    {
        await _datasetService.RevokeAccessAsync(AccessContext, id, grantId, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/exports")]
    public async Task<ActionResult<HeritageDatasetExportDto>> CreateExport(
        Guid id, CreateHeritageDatasetExportRequest request, CancellationToken cancellationToken)
        => Ok(await _datasetService.CreateExportAsync(AccessContext, id, request, cancellationToken));

    [HttpGet("{id:guid}/exports")]
    public async Task<ActionResult<PagedResult<HeritageDatasetExportDto>>> GetExports(
        Guid id, [FromQuery] HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetExportsAsync(AccessContext, id, query, cancellationToken));

    [HttpGet("{id:guid}/export-analytics")]
    public async Task<ActionResult<HeritageDatasetExportAnalyticsDto>> GetExportAnalytics(Guid id, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetExportAnalyticsAsync(AccessContext, id, cancellationToken));

    [HttpGet("/api/heritage-database/exports/mine")]
    public async Task<ActionResult<PagedResult<HeritageDatasetExportDto>>> GetMyExports(
        [FromQuery] HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _datasetService.GetMyExportsAsync(CurrentUserId, query, cancellationToken));
}
