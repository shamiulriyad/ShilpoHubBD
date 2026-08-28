using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/supplier-matching")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class SupplierMatchingController : ControllerBase
{
    private readonly ISupplierMatchingService _supplierMatchingService;

    public SupplierMatchingController(ISupplierMatchingService supplierMatchingService)
    {
        _supplierMatchingService = supplierMatchingService;
    }

    [HttpPost("match")]
    public async Task<ActionResult<List<SupplierMatchResultDto>>> Match(SupplierMatchRequest request, CancellationToken cancellationToken)
    {
        var result = await _supplierMatchingService.MatchAsync(request, cancellationToken);
        return Ok(result);
    }
}
