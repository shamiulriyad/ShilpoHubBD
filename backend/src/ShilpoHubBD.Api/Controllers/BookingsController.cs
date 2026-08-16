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
[Route("api/bookings")]
[Authorize]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpPost]
    public async Task<ActionResult<BookingDto>> Create(CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var result = await _bookingService.CreateAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<BookingDto>>> GetMine(
        [FromQuery] BookingQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _bookingService.GetMyBookingsAsync(CurrentUserId, query, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpGet("provider")]
    public async Task<ActionResult<PagedResult<BookingDto>>> GetProvider(
        [FromQuery] BookingQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _bookingService.GetProviderBookingsAsync(CurrentUserId, query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _bookingService.GetByIdAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{id:guid}/confirm")]
    public async Task<ActionResult<BookingDto>> Confirm(Guid id, CancellationToken cancellationToken)
        => Ok(await _bookingService.ConfirmAsync(CurrentUserId, IsAdmin, id, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{id:guid}/reject")]
    public async Task<ActionResult<BookingDto>> Reject(Guid id, CancelBookingRequest request, CancellationToken cancellationToken)
        => Ok(await _bookingService.RejectAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [HttpPut("{id:guid}/cancel")]
    public async Task<ActionResult<BookingDto>> Cancel(Guid id, CancelBookingRequest request, CancellationToken cancellationToken)
        => Ok(await _bookingService.CancelAsync(CurrentUserId, IsAdmin, id, request, cancellationToken));

    [Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [HttpPut("{id:guid}/complete")]
    public async Task<ActionResult<BookingDto>> Complete(Guid id, CancellationToken cancellationToken)
        => Ok(await _bookingService.CompleteAsync(CurrentUserId, IsAdmin, id, cancellationToken));
}
