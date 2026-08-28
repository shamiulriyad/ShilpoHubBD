using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/supplier-discovery")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class SupplierDiscoveryController : ControllerBase
{
    private readonly ISupplierDiscoveryService _supplierDiscoveryService;

    public SupplierDiscoveryController(ISupplierDiscoveryService supplierDiscoveryService)
    {
        _supplierDiscoveryService = supplierDiscoveryService;
    }

    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<SupplierSearchResultDto>>> Search(
        [FromQuery] SupplierSearchParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _supplierDiscoveryService.SearchAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("producers/{producerId:guid}")]
    public async Task<ActionResult<SupplierProfileDto>> GetProducerProfile(Guid producerId, CancellationToken cancellationToken)
    {
        var result = await _supplierDiscoveryService.GetProducerProfileAsync(producerId, cancellationToken);
        return Ok(result);
    }
}
