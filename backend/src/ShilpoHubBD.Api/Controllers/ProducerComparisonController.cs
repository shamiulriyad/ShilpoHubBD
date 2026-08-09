using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.ProducerComparison;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producer-comparison")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class ProducerComparisonController : ControllerBase
{
    private readonly IProducerComparisonService _producerComparisonService;

    public ProducerComparisonController(IProducerComparisonService producerComparisonService)
    {
        _producerComparisonService = producerComparisonService;
    }

    [HttpPost("compare")]
    public async Task<ActionResult<List<ProducerComparisonRowDto>>> Compare(ProducerComparisonRequest request, CancellationToken cancellationToken)
    {
        var result = await _producerComparisonService.CompareAsync(request, cancellationToken);
        return Ok(result);
    }
}
