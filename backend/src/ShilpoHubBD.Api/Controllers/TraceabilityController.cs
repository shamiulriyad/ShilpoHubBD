using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/traceability")]
public class TraceabilityController : ControllerBase
{
    private readonly ITraceabilityService _traceabilityService;

    public TraceabilityController(ITraceabilityService traceabilityService)
    {
        _traceabilityService = traceabilityService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("products/{productId:guid}")]
    public async Task<ActionResult<ProductTraceabilityDto>> GetByProduct(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _traceabilityService.GetByProductIdAsync(productId, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<ProductTraceabilityDto>> Create(CreateProductTraceabilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _traceabilityService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetByProduct), new { productId = result.ProductId }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProductTraceabilityDto>> Update(Guid id, UpdateProductTraceabilityRequest request, CancellationToken cancellationToken)
    {
        var result = await _traceabilityService.UpdateAsync(id, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _traceabilityService.DeleteAsync(id, CurrentUserId, IsAdmin, cancellationToken);
        return NoContent();
    }
}
