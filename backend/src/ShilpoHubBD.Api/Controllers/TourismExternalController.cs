using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Tourism;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// "Where to stay" and "More places to explore & eat". Both read from our own database, which is
// topped up from OpenStreetMap during the request; the client never talks to Overpass.
[ApiController]
[Route("api/tourism")]
public class TourismExternalController : ControllerBase
{
    private readonly IAccommodationService _accommodations;
    private readonly ITourismPoiService _pois;
    private readonly ITourismExternalSyncService _sync;

    public TourismExternalController(IAccommodationService accommodations, ITourismPoiService pois, ITourismExternalSyncService sync)
    {
        _accommodations = accommodations;
        _pois = pois;
        _sync = sync;
    }

    // GET /api/tourism/accommodations?districtId={guid}   or   ?lat=&lon=&radius=  (km, stored data only)
    [HttpGet("accommodations")]
    public async Task<ActionResult<List<TourismLocationDto>>> Accommodations(
        [FromQuery] Guid? districtId, [FromQuery] double? lat, [FromQuery] double? lon, [FromQuery] double? radius, CancellationToken cancellationToken)
    {
        if (districtId.HasValue) return Ok(await _accommodations.GetByDistrictAsync(districtId.Value, cancellationToken));
        if (lat.HasValue && lon.HasValue) return Ok(await _accommodations.GetNearbyAsync(lat.Value, lon.Value, radius, cancellationToken));
        return BadRequest("Provide districtId, or lat and lon.");
    }

    // Hotels, hostels, resorts and guest houses within radiusKm of the destination's coordinates,
    // nearest first, each with its distance. Fetched from OpenStreetMap on demand (cached in our DB).
    [HttpGet("accommodations/nearby")]
    public async Task<ActionResult<NearbyAccommodationsDto>> AccommodationsNearby(
        [FromQuery] Guid districtId, [FromQuery] double? radiusKm, CancellationToken cancellationToken)
        => Ok(await _accommodations.GetAroundDestinationAsync(districtId, radiusKm, cancellationToken));

    [HttpGet("pois")]
    public async Task<ActionResult<List<TourismLocationDto>>> Pois(
        [FromQuery] Guid? districtId, [FromQuery] double? lat, [FromQuery] double? lon, [FromQuery] double? radius, CancellationToken cancellationToken)
    {
        if (districtId.HasValue) return Ok(await _pois.GetByDistrictAsync(districtId.Value, cancellationToken));
        if (lat.HasValue && lon.HasValue) return Ok(await _pois.GetNearbyAsync(lat.Value, lon.Value, radius, cancellationToken));
        return BadRequest("Provide districtId, or lat and lon.");
    }

    // Admin: refresh one district's OpenStreetMap data now, ignoring the freshness window.
    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("sync")]
    public async Task<ActionResult<TourismSyncResultDto>> Sync([FromQuery] Guid districtId, CancellationToken cancellationToken)
        => Ok(await _sync.SyncAsync(districtId, force: true, cancellationToken));
}
