using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Security;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/admin/security/api-keys")]
public class ApiKeysController : ControllerBase
{
    private readonly IApiKeyService _service;

    public ApiKeysController(IApiKeyService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<ActionResult<CreateApiKeyResultDto>> Create(CreateApiKeyRequest request, CancellationToken cancellationToken)
        => Ok(await _service.CreateAsync(CurrentUserId, request, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<PagedResult<ApiKeyDto>>> GetPaged(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
        => Ok(await _service.GetPagedAsync(page, pageSize, cancellationToken));

    [HttpPost("{id:guid}/revoke")]
    public async Task<ActionResult<ApiKeyDto>> Revoke(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.RevokeAsync(id, cancellationToken));
}
