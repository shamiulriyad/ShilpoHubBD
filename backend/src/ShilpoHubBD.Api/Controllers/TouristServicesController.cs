using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.TouristBooking;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/tourist-services")]
public class TouristServicesController : ControllerBase
{
    private readonly ITouristServiceService _serviceService;
    private readonly IServiceAvailabilityService _availabilityService;

    public TouristServicesController(ITouristServiceService serviceService, IServiceAvailabilityService availabilityService)
    {
        _serviceService = serviceService;
        _availabilityService = availabilityService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet]
    public async Task<ActionResult<PagedResult<TouristServiceDto>>> GetPaged(
        [FromQuery] TouristServiceQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _serviceService.GetPagedAsync(query, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<TouristServiceDto>>> GetMine(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
        => Ok(await _serviceService.GetMineAsync(CurrentUserId, page, pageSize, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TouristServiceDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _serviceService.GetByIdAsync(id, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost]
    public async Task<ActionResult<TouristServiceDto>> Create(CreateTouristServiceRequest request, CancellationToken cancellationToken)
    {
        var result = await _serviceService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<TouristServiceDto>> Update(Guid id, UpdateTouristServiceRequest request, CancellationToken cancellationToken)
        => Ok(await _serviceService.UpdateAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _serviceService.DeleteAsync(CurrentUserId, IsAdmin, id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{serviceId:guid}/availability-slots")]
    public async Task<ActionResult<PagedResult<ServiceAvailabilitySlotDto>>> GetAvailabilitySlots(
        Guid serviceId, [FromQuery] AvailabilitySlotQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _availabilityService.GetPagedByServiceAsync(serviceId, query, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPost("{serviceId:guid}/availability-slots")]
    public async Task<ActionResult<ServiceAvailabilitySlotDto>> CreateAvailabilitySlot(
        Guid serviceId, CreateServiceAvailabilitySlotRequest request, CancellationToken cancellationToken)
    {
        var result = await _availabilityService.CreateAsync(CurrentUserId, serviceId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAvailabilitySlots), new { serviceId }, result);
    }

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("availability-slots/{slotId:guid}")]
    public async Task<ActionResult<ServiceAvailabilitySlotDto>> UpdateAvailabilitySlot(
        Guid slotId, UpdateServiceAvailabilitySlotRequest request, CancellationToken cancellationToken)
        => Ok(await _availabilityService.UpdateAsync(CurrentUserId, IsAdmin, slotId, request, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpDelete("availability-slots/{slotId:guid}")]
    public async Task<IActionResult> DeleteAvailabilitySlot(Guid slotId, CancellationToken cancellationToken)
    {
        await _availabilityService.DeleteAsync(CurrentUserId, IsAdmin, slotId, cancellationToken);
        return NoContent();
    }
}
