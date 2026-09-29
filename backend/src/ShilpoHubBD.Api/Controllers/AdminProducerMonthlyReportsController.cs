using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

// SuperAdmin only — full, unrestricted analytics. The restricted, share-gated view for Government/NGO
// users lives on GovProducerMonthlyReportsController instead.
[ApiController]
[Authorize(Roles = RoleNames.SuperAdmin)]
[Route("api/admin/producer-monthly-reports")]
public class AdminProducerMonthlyReportsController : ControllerBase
{
    private readonly IProducerMonthlyReportService _service;

    public AdminProducerMonthlyReportsController(IProducerMonthlyReportService service)
    {
        _service = service;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost("generate")]
    public async Task<ActionResult<ProducerMonthlyReportGenerationResultDto>> Generate(
        [FromBody] GenerateProducerMonthlyReportRequest request, CancellationToken cancellationToken)
        => Ok(await _service.GenerateForMonthAsync(request.Year, request.Month, cancellationToken));

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProducerMonthlyReportDto>>> GetPaged(
        [FromQuery] ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetPagedAsync(query, cancellationToken));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProducerMonthlyReportDto>> GetById(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetByIdAsync(id, cancellationToken));

    [HttpGet("producer/{producerId:guid}")]
    public async Task<ActionResult<ProducerMonthlyReportDto>> GetForProducer(
        Guid producerId, [FromQuery] int year, [FromQuery] int month, CancellationToken cancellationToken)
        => Ok(await _service.GetForProducerAsync(producerId, year, month, cancellationToken));

    [HttpGet("producer/{producerId:guid}/compare")]
    public async Task<ActionResult<ProducerMonthlyReportComparisonDto>> Compare(
        Guid producerId, [FromQuery] int? year, [FromQuery] int? month, CancellationToken cancellationToken)
        => Ok(await _service.CompareAsync(producerId, year, month, cancellationToken));

    [HttpPost("share")]
    public async Task<ActionResult<ProducerMonthlyReportShareResultDto>> Share(
        [FromBody] ShareProducerMonthlyReportsRequest request, CancellationToken cancellationToken)
        => Ok(await _service.ShareAsync(CurrentUserId, request, cancellationToken));

    [HttpGet("{id:guid}/shares")]
    public async Task<ActionResult<List<ProducerMonthlyReportShareDto>>> GetShares(Guid id, CancellationToken cancellationToken)
        => Ok(await _service.GetSharesForReportAsync(id, cancellationToken));

    [HttpGet("intelligence")]
    public async Task<ActionResult<PagedResult<ProducerIntelligenceRowDto>>> GetIntelligenceList(
        [FromQuery] ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetIntelligenceListAsync(query, cancellationToken));

    [HttpGet("intelligence/dashboard")]
    public async Task<ActionResult<ProducerIntelligenceDashboardDto>> GetIntelligenceDashboard(
        [FromQuery] ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _service.GetIntelligenceDashboardAsync(query, cancellationToken));
}
