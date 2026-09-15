using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/districts")]
public class DistrictsController : ControllerBase
{
    private readonly IDistrictService _districtService;

    public DistrictsController(IDistrictService districtService)
    {
        _districtService = districtService;
    }

    [HttpGet]
    public async Task<ActionResult<List<DistrictDto>>> GetAll(
        [FromQuery] bool includeInactive, CancellationToken cancellationToken)
    {
        var result = await _districtService.GetAllAsync(includeInactive, cancellationToken);
        return Ok(result);
    }

    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<DistrictDto>> Update(
        Guid id, UpdateDistrictRequest request, CancellationToken cancellationToken)
        => Ok(await _districtService.UpdateAsync(id, request, cancellationToken));
}
