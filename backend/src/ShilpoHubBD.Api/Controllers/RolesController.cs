using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Roles;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("assign")]
    public async Task<IActionResult> AssignRole(AssignRoleRequest request, CancellationToken cancellationToken)
    {
        await _roleService.AssignRoleAsync(request.UserId, request.Role, CurrentUserId, cancellationToken);
        return Ok(new MessageResponse { Success = true, Message = "Role assigned successfully." });
    }

    [HttpPost("remove")]
    public async Task<IActionResult> RemoveRole(RemoveRoleRequest request, CancellationToken cancellationToken)
    {
        await _roleService.RemoveRoleAsync(request.UserId, request.Role, cancellationToken);
        return Ok(new MessageResponse { Success = true, Message = "Role removed successfully." });
    }
}
