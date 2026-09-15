using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public PermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("api/admin/permissions")]
    public async Task<ActionResult<List<PermissionDto>>> GetAll(CancellationToken cancellationToken)
        => Ok(await _permissionService.GetAllAsync(cancellationToken));

    [HttpPost("api/admin/permissions")]
    public async Task<ActionResult<PermissionDto>> Create(
        CreatePermissionRequest request, CancellationToken cancellationToken)
    {
        var result = await _permissionService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), result);
    }

    [HttpDelete("api/admin/permissions/{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _permissionService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("api/admin/roles")]
    public async Task<ActionResult<List<RoleAdminDto>>> GetRoles(CancellationToken cancellationToken)
        => Ok(await _permissionService.GetRolesAsync(cancellationToken));

    [HttpGet("api/admin/roles/{roleId:guid}/permissions")]
    public async Task<ActionResult<RolePermissionsDto>> GetRolePermissions(
        Guid roleId, CancellationToken cancellationToken)
        => Ok(await _permissionService.GetRolePermissionsAsync(roleId, cancellationToken));

    [HttpPut("api/admin/roles/{roleId:guid}/permissions")]
    public async Task<ActionResult<RolePermissionsDto>> SyncRolePermissions(
        Guid roleId, SyncRolePermissionsRequest request, CancellationToken cancellationToken)
        => Ok(await _permissionService.SyncRolePermissionsAsync(roleId, CurrentUserId, request, cancellationToken));
}
