using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/logistics/warehouse-stock")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class WarehouseStockController : ControllerBase
{
    private readonly IWarehouseStockService _service;

    public WarehouseStockController(IWarehouseStockService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<WarehouseStockItemListItemDto>>> GetStockItems(
        [FromQuery] WarehouseStockQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetStockItemsAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("movements")]
    public async Task<ActionResult<PagedResult<WarehouseStockMovementDto>>> GetMovements(
        [FromQuery] WarehouseStockMovementQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetMovementsAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseStockItemDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetStockItemByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpPost("receive")]
    public async Task<ActionResult<WarehouseStockItemDto>> Receive(
        ReceiveStockRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.ReceiveAsync(CurrentUserId, IsAdmin, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPost("{id:guid}/issue")]
    public async Task<ActionResult<WarehouseStockItemDto>> Issue(
        Guid id, IssueStockRequest request, CancellationToken cancellationToken)
        => Ok(await _service.IssueAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/transfer")]
    public async Task<ActionResult<WarehouseStockItemDto>> Transfer(
        Guid id, TransferStockRequest request, CancellationToken cancellationToken)
        => Ok(await _service.TransferAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/adjust")]
    public async Task<ActionResult<WarehouseStockItemDto>> Adjust(
        Guid id, AdjustStockRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AdjustAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/reserve")]
    public async Task<ActionResult<WarehouseStockItemDto>> Reserve(
        Guid id, ReserveStockRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ReserveAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPost("{id:guid}/release")]
    public async Task<ActionResult<WarehouseStockItemDto>> Release(
        Guid id, ReserveStockRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ReleaseReservationAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteStockItemAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }
}
