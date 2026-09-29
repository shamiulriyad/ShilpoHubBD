using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// Restricted view: a Government/NGO user only ever sees reports an Admin has explicitly shared with
// them (GetPaged forces SharedWithUserId to the caller's own id; GetById checks a share row exists
// before returning anything) — never the full Producer analytics AdminProducerMonthlyReportsController
// exposes to SuperAdmin.
[ApiController]
[Authorize(Roles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/governance/producer-monthly-reports")]
public class GovProducerMonthlyReportsController : ControllerBase
{
    private readonly IProducerMonthlyReportService _service;

    public GovProducerMonthlyReportsController(IProducerMonthlyReportService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProducerMonthlyReportDto>>> GetPaged(
        [FromQuery] ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
    {
        // Never trust a client-supplied SharedWithUserId — always the caller's own id.
        query.SharedWithUserId = CurrentUserId;
        return Ok(await _service.GetPagedAsync(query, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProducerMonthlyReportDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetSharedReportAsync(id, CurrentUserId, cancellationToken));
}
