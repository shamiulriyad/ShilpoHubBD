using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Authorize(Roles = $"{RoleNames.GovernmentNGO},{RoleNames.SuperAdmin}")]
[Route("api/governance/monitoring")]
public class MonitoringController : ControllerBase
{
    private readonly IMonitoringService _monitoringService;
    private readonly IProductModerationAdminService _productModerationAdminService;

    public MonitoringController(IMonitoringService monitoringService, IProductModerationAdminService productModerationAdminService)
    {
        _monitoringService = monitoringService;
        _productModerationAdminService = productModerationAdminService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Run the rule-based fraud / fake-product / review-abuse / QR-anomaly scans.</summary>
    [HttpPost("scans")]
    public async Task<ActionResult<MonitoringScanResultDto>> RunScan(
        RunMonitoringScanRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.RunScanAsync(CurrentUserId, request, cancellationToken));

    [HttpGet("flags")]
    public async Task<ActionResult<PagedResult<MonitoringFlagListItemDto>>> GetFlags(
        [FromQuery] MonitoringFlagQueryParameters query, CancellationToken cancellationToken)
        => Ok(await _monitoringService.GetFlagsAsync(query, cancellationToken));

    [HttpGet("flags/{id:guid}")]
    public async Task<ActionResult<MonitoringFlagDto>> GetFlag(Guid id, CancellationToken cancellationToken)
        => Ok(await _monitoringService.GetFlagByIdAsync(id, cancellationToken));

    [HttpPost("flags")]
    public async Task<ActionResult<MonitoringFlagDto>> CreateFlag(
        CreateMonitoringFlagRequest request, CancellationToken cancellationToken)
    {
        var result = await _monitoringService.CreateFlagAsync(CurrentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetFlag), new { id = result.Id }, result);
    }

    [HttpPost("flags/{id:guid}/status")]
    public async Task<ActionResult<MonitoringFlagDto>> UpdateFlagStatus(
        Guid id, UpdateMonitoringFlagStatusRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.UpdateFlagStatusAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("flags/{id:guid}/assign")]
    public async Task<ActionResult<MonitoringFlagDto>> AssignFlag(
        Guid id, AssignMonitoringFlagRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.AssignFlagAsync(CurrentUserId, id, request, cancellationToken));

    [HttpPost("flags/{id:guid}/notes")]
    public async Task<ActionResult<MonitoringFlagDto>> AddFlagNote(
        Guid id, AddMonitoringFlagNoteRequest request, CancellationToken cancellationToken)
        => Ok(await _monitoringService.AddFlagNoteAsync(CurrentUserId, id, request, cancellationToken));

    [HttpDelete("flags/{id:guid}")]
    public async Task<IActionResult> DeleteFlag(Guid id, CancellationToken cancellationToken)
    {
        await _monitoringService.DeleteFlagAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>QR-verification volume, invalid-scan rate and anomalous products.</summary>
    [HttpGet("qr/overview")]
    public async Task<ActionResult<QrMonitoringOverviewDto>> GetQrOverview(
        [FromQuery] DateTime? from, [FromQuery] DateTime? to, CancellationToken cancellationToken)
        => Ok(await _monitoringService.GetQrOverviewAsync(from, to, cancellationToken));

    // ---- Product-review moderation (Part 3): the admin list/detail/ban for RepeatedProductComplaints cases ---

    /// <summary>Admin moderation list: product/producer/risk/complaint counts, joined live (not a frozen snapshot).</summary>
    [HttpGet("product-moderation-cases")]
    public async Task<ActionResult<PagedResult<ProductModerationCaseListItemDto>>> GetProductModerationCases(
        [FromQuery] string? riskState, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
        => Ok(await _productModerationAdminService.GetCasesAsync(riskState, status, page, pageSize, cancellationToken));

    /// <summary>Full evidence for one case: review statistics, the triggering review, similar historical
    /// reviews, the AI analysis, and moderation history — not just the AI's own summary.</summary>
    [HttpGet("flags/{id:guid}/product-case")]
    public async Task<ActionResult<ProductModerationCaseDto>> GetProductModerationCase(Guid id, CancellationToken cancellationToken)
        => Ok(await _productModerationAdminService.GetCaseDetailAsync(id, cancellationToken));

    /// <summary>
    /// Bans the product behind a moderation case. SuperAdmin only — this attribute combines with the
    /// controller-level [Authorize] above (both must pass), so a GovernmentNGO account, which can view and
    /// triage cases, cannot ban a product. AI never calls this; it only ever runs from an explicit admin request.
    /// </summary>
    [Authorize(Roles = RoleNames.SuperAdmin)]
    [HttpPost("flags/{id:guid}/ban-product")]
    public async Task<ActionResult<MonitoringFlagDto>> BanProduct(Guid id, BanProductRequest request, CancellationToken cancellationToken)
        => Ok(await _productModerationAdminService.BanProductAsync(CurrentUserId, id, request, cancellationToken));
}
