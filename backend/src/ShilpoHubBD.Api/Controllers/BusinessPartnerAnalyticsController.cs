using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/business-partner-analytics")]
[Authorize(Roles = $"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}")]
public class BusinessPartnerAnalyticsController : ControllerBase
{
    private readonly IBusinessPartnerAnalyticsService _analyticsService;

    public BusinessPartnerAnalyticsController(IBusinessPartnerAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin => User.IsInRole(RoleNames.SuperAdmin);

    [HttpGet("market-demand")]
    public async Task<ActionResult<List<CategoryDemandDto>>> GetMarketDemand(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetMarketDemandAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("export-trends")]
    public async Task<ActionResult<List<MonthlyTrendDto>>> GetExportTrends(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetExportTrendsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("production-forecast")]
    public async Task<ActionResult<ProductionForecastDto>> GetProductionForecast(
        [FromQuery] Guid categoryId, [FromQuery] int horizonMonths = 3, CancellationToken cancellationToken = default)
    {
        var result = await _analyticsService.GetProductionForecastAsync(categoryId, horizonMonths, cancellationToken);
        return Ok(result);
    }

    [HttpGet("industry-insights")]
    public async Task<ActionResult<List<IndustryInsightDto>>> GetIndustryInsights(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetIndustryInsightsAsync(parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("procurement")]
    public async Task<ActionResult<ProcurementAnalyticsDto>> GetProcurementAnalytics(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetProcurementAnalyticsAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("supplier-performance")]
    public async Task<ActionResult<List<SupplierPerformanceDto>>> GetSupplierPerformance(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetSupplierPerformanceAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("spending")]
    public async Task<ActionResult<SpendingAnalyticsDto>> GetSpendingAnalytics(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetSpendingAnalyticsAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }

    [HttpGet("order-trends")]
    public async Task<ActionResult<List<MonthlyTrendDto>>> GetOrderTrends(
        [FromQuery] AnalyticsQueryParameters parameters, CancellationToken cancellationToken)
    {
        var result = await _analyticsService.GetOrderTrendsAsync(CurrentUserId, IsAdmin, parameters, cancellationToken);
        return Ok(result);
    }
}
