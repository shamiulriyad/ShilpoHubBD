using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/admin/security/system-health")]
public class SystemHealthController : ControllerBase
{
    private readonly ISystemHealthService _service;

    public SystemHealthController(ISystemHealthService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<SystemHealthDto>> Get(CancellationToken cancellationToken)
        => Ok(await _service.GetHealthAsync(cancellationToken));
}
