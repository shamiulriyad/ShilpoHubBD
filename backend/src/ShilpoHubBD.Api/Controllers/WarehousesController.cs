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
[Route("api/logistics/warehouses")]
[Authorize(Roles = $"{RoleNames.LogisticsPartner},{RoleNames.SuperAdmin}")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _service;

    public WarehousesController(IWarehouseService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<WarehouseListItemDto>>> GetPaged(
        [FromQuery] WarehouseQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(CurrentUserId, IsAdmin, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [HttpPost]
    public async Task<ActionResult<WarehouseDto>> Create(
        CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateAsync(CurrentUserId, IsAdmin, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<WarehouseDto>> Update(
        Guid id, UpdateWarehouseRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _service.DeleteAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/zones")]
    public async Task<ActionResult<WarehouseDto>> AddZone(
        Guid id, UpsertWarehouseZoneRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddZoneAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPut("{id:guid}/zones/{zoneId:guid}")]
    public async Task<ActionResult<WarehouseDto>> UpdateZone(
        Guid id, Guid zoneId, UpsertWarehouseZoneRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateZoneAsync(CurrentUserId, IsAdmin, id, zoneId, request, cancellationToken));

    [HttpDelete("{id:guid}/zones/{zoneId:guid}")]
    public async Task<ActionResult<WarehouseDto>> RemoveZone(
        Guid id, Guid zoneId, CancellationToken cancellationToken)
        => Ok(await _service.RemoveZoneAsync(CurrentUserId, IsAdmin, id, zoneId, cancellationToken));

    [HttpPost("{id:guid}/bins")]
    public async Task<ActionResult<WarehouseDto>> AddBin(
        Guid id, UpsertWarehouseBinRequest request, CancellationToken cancellationToken)
        => Ok(await _service.AddBinAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPut("{id:guid}/bins/{binId:guid}")]
    public async Task<ActionResult<WarehouseDto>> UpdateBin(
        Guid id, Guid binId, UpsertWarehouseBinRequest request, CancellationToken cancellationToken)
        => Ok(await _service.UpdateBinAsync(CurrentUserId, IsAdmin, id, binId, request, cancellationToken));

    [HttpDelete("{id:guid}/bins/{binId:guid}")]
    public async Task<ActionResult<WarehouseDto>> RemoveBin(
        Guid id, Guid binId, CancellationToken cancellationToken)
        => Ok(await _service.RemoveBinAsync(CurrentUserId, IsAdmin, id, binId, cancellationToken));
}
