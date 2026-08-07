using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Interfaces.Services;

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
    public async Task<ActionResult<List<DistrictDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _districtService.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
