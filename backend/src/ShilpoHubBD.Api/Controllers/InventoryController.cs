using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Inventory;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/inventory")]
[Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost("products/{productId:guid}/adjust")]
    public async Task<ActionResult<InventoryTransactionDto>> AdjustStock(
        Guid productId, AdjustStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.AdjustStockAsync(productId, CurrentUserId, IsAdmin, request, cancellationToken);
        return Ok(result);
    }

    [HttpGet("products/{productId:guid}/history")]
    public async Task<ActionResult<List<InventoryTransactionDto>>> GetHistory(Guid productId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetHistoryAsync(productId, CurrentUserId, IsAdmin, cancellationToken);
        return Ok(result);
    }

    [HttpGet("low-stock")]
    public async Task<ActionResult<List<LowStockProductDto>>> GetLowStock(
        [FromQuery] Guid? producerId, CancellationToken cancellationToken)
    {
        var targetProducerId = IsAdmin && producerId.HasValue ? producerId.Value : CurrentUserId;
        var result = await _inventoryService.GetLowStockAsync(targetProducerId, cancellationToken);
        return Ok(result);
    }
}
