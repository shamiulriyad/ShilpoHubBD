using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Profiles;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Real-world profile (name, expertise, location, phone, NID) of the signed-in member, plus the admin review of it.
[ApiController]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IUserProfileService _service;
    private readonly ShilpoHubBD.Application.Interfaces.Repositories.IUserProfileRepository _repository;

    public ProfileController(IUserProfileService service, ShilpoHubBD.Application.Interfaces.Repositories.IUserProfileRepository repository)
    {
        _service = service;
        _repository = repository;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    // Expertise values of approved producers, for the "filter producers by expertise" dropdowns.
    [AllowAnonymous]
    [HttpGet("api/profile/expertise-options")]
    public async Task<ActionResult<List<string>>> ExpertiseOptions(CancellationToken cancellationToken)
        => Ok(await _repository.GetProducerExpertiseOptionsAsync(cancellationToken));

    [HttpGet("api/profile/me")]
    public async Task<ActionResult<UserProfileDto>> GetMine(CancellationToken cancellationToken)
        => Ok(await _service.GetMineAsync(CurrentUserId, cancellationToken));

    [HttpPut("api/profile/me")]
    public async Task<ActionResult<UserProfileDto>> UpsertMine(UpsertUserProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpsertMineAsync(CurrentUserId, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpGet("api/admin/profiles")]
    public async Task<ActionResult<PagedResult<UserProfileListItemDto>>> GetForAdmin(
        [FromQuery] UserProfileQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetForAdminAsync(query, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("api/admin/profiles/{id:guid}/approve")]
    public async Task<ActionResult<UserProfileListItemDto>> Approve(Guid id, ReviewUserProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ApproveAsync(id, CurrentUserId, request, cancellationToken));

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("api/admin/profiles/{id:guid}/reject")]
    public async Task<ActionResult<UserProfileListItemDto>> Reject(Guid id, ReviewUserProfileRequest request, CancellationToken cancellationToken)
        => Ok(await _service.RejectAsync(id, CurrentUserId, request, cancellationToken));
}
