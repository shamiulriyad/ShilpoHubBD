using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

/// <summary>Live, read-only heritage data for research and analysis. Restricted to research roles.</summary>
[ApiController]
[Authorize(Roles = $"{RoleNames.HeritageInnovationHub},{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/heritage-database/live")]
public class HeritageDataController : ControllerBase
{
    private readonly IHeritageDataService _dataService;

    public HeritageDataController(IHeritageDataService dataService)
    {
        _dataService = dataService;
    }

    [HttpGet("locations")]
    public async Task<ActionResult<PagedResult<HeritageLocationRecordDto>>> GetLocations(
        [FromQuery] LiveHeritageQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dataService.GetLocationsAsync(query, cancellationToken));

    [HttpGet("villages")]
    public async Task<ActionResult<PagedResult<HeritageVillageRecordDto>>> GetVillages(
        [FromQuery] LiveHeritageQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dataService.GetVillagesAsync(query, cancellationToken));

    [HttpGet("producers")]
    public async Task<ActionResult<PagedResult<HeritageProducerRecordDto>>> GetProducers(
        [FromQuery] LiveHeritageQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dataService.GetProducersAsync(query, cancellationToken));

    [HttpGet("products")]
    public async Task<ActionResult<PagedResult<HeritageProductRecordDto>>> GetProducts(
        [FromQuery] LiveHeritageQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dataService.GetProductsAsync(query, cancellationToken));

    [HttpGet("tourism")]
    public async Task<ActionResult<PagedResult<HeritageTourismRecordDto>>> GetTourism(
        [FromQuery] LiveHeritageQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _dataService.GetTourismAsync(query, cancellationToken));

    [HttpGet("demographics")]
    public async Task<ActionResult<ProducerDemographicsDto>> GetProducerDemographics(CancellationToken cancellationToken)
        => Ok(await _dataService.GetProducerDemographicsAsync(cancellationToken));

    [HttpGet("summary")]
    public async Task<ActionResult<HeritageDatabaseSummaryDto>> GetSummary(CancellationToken cancellationToken)
        => Ok(await _dataService.GetSummaryAsync(cancellationToken));
}
