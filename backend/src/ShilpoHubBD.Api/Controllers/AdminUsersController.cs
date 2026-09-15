using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/admin/users")]
public class AdminUsersController : ControllerBase
{
    private readonly IAdminUserService _adminUserService;

    public AdminUsersController(IAdminUserService adminUserService)
    {
        _adminUserService = adminUserService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResult<AdminUserListItemDto>>> GetPaged(
        [FromQuery] AdminUserQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _adminUserService.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AdminUserDetailDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _adminUserService.GetByIdAsync(id, cancellationToken));

    [HttpPost("{id:guid}/activate")]
    public async Task<ActionResult<AdminUserDetailDto>> Activate(Guid id, CancellationToken cancellationToken)
        => Ok(await _adminUserService.SetActiveAsync(id, true, cancellationToken));

    [HttpPost("{id:guid}/deactivate")]
    public async Task<ActionResult<AdminUserDetailDto>> Deactivate(Guid id, CancellationToken cancellationToken)
        => Ok(await _adminUserService.SetActiveAsync(id, false, cancellationToken));
}
