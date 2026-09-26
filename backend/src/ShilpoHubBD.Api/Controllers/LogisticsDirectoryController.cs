using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Lets a producer see which verified logistics partners are taking work, so a shipment can be handed
// to one of them when an order item is shipped. (The partner management endpoints under
// api/logistics/partners are for partners and admins only.)
[ApiController]
[Route("api/logistics/directory")]
[Authorize(Roles = $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
public class LogisticsDirectoryController : ControllerBase
{
    private readonly ILogisticsPartnerRepository _partnerRepository;
    private readonly IRouteOptimizationRepository _routeRepository;

    public LogisticsDirectoryController(ILogisticsPartnerRepository partnerRepository, IRouteOptimizationRepository routeRepository)
    {
        _partnerRepository = partnerRepository;
        _routeRepository = routeRepository;
    }

    [HttpGet]
    public async Task<ActionResult<List<LogisticsPartnerDirectoryItemDto>>> GetAvailable(CancellationToken cancellationToken)
    {
        var partners = await _partnerRepository.GetAvailableForHandoffAsync(cancellationToken);
        return Ok(partners.Select(p => new LogisticsPartnerDirectoryItemDto
        {
            ProfileId = p.Id,
            CompanyName = p.CompanyName,
            BaseCity = p.BaseCity,
            BaseDistrictName = p.BaseDistrict?.Name,
            OffersCashOnDelivery = p.OffersCashOnDelivery,
            OffersFragileHandling = p.OffersFragileHandling,
        }).ToList());
    }

    [HttpGet("{profileId:guid}/routes")]
    public async Task<ActionResult<List<LogisticsRouteOptionDto>>> GetRoutes(Guid profileId, CancellationToken cancellationToken)
    {
        var routes = await _routeRepository.GetOpenForProfileAsync(profileId, cancellationToken);
        return Ok(routes.Select(r => new LogisticsRouteOptionDto
        {
            RouteId = r.Id,
            RouteCode = r.RouteCode,
            Name = r.Name,
            Status = r.Status.ToString(),
            From = r.StartLocationLabel,
            To = r.EndLocationLabel,
            OriginDistrictName = r.OriginDistrict?.Name,
            ScheduledDate = r.ScheduledDate,
            VehicleCapacityKg = r.VehicleCapacityKg,
            TotalDistanceKm = r.TotalDistanceKm,
            TotalStops = r.TotalStops,
            Notes = r.Notes,
        }).ToList());
    }
}
