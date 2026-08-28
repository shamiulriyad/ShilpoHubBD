using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/governance/complaints")]
public class ComplaintsController : ControllerBase
{
    private readonly IComplaintService _complaintService;

    public ComplaintsController(IComplaintService complaintService)
    {
        _complaintService = complaintService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ComplaintListItemDto>>> GetPaged(
        [FromQuery] ComplaintQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _complaintService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ComplaintDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _complaintService.GetByIdAsync(id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<ComplaintDto>> Create(
        CreateComplaintRequest request, CancellationToken cancellationToken)
    {
        var result = await _complaintService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ComplaintDto>> Update(
        Guid id, UpdateComplaintRequest request, CancellationToken cancellationToken)
        => Ok(await _complaintService.UpdateAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/updates")]
    public async Task<ActionResult<ComplaintDto>> AddUpdate(
        Guid id, AddComplaintUpdateRequest request, CancellationToken cancellationToken)
        => Ok(await _complaintService.AddUpdateAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/assign")]
    public async Task<ActionResult<ComplaintDto>> Assign(
        Guid id, AssignComplaintRequest request, CancellationToken cancellationToken)
        => Ok(await _complaintService.AssignAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/resolve")]
    public async Task<ActionResult<ComplaintDto>> Resolve(
        Guid id, ResolveComplaintRequest request, CancellationToken cancellationToken)
        => Ok(await _complaintService.ResolveAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("{id:guid}/link-flag")]
    public async Task<ActionResult<ComplaintDto>> LinkFlag(
        Guid id, LinkComplaintFlagRequest request, CancellationToken cancellationToken)
        => Ok(await _complaintService.LinkFlagAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _complaintService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
