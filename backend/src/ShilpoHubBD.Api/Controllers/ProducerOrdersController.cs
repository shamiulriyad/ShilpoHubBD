using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;

namespace ShilpoHubBD.Api.Controllers;

[ApiController]
[Route("api/producer/orders")]
[Authorize(Roles = RoleNames.Producer)]
public class ProducerOrdersController : ControllerBase
{
    private readonly IProducerOrderService _producerOrderService;

    public ProducerOrdersController(IProducerOrderService producerOrderService)
    {
        _producerOrderService = producerOrderService;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<PagedResult<ProducerOrderItemDto>>> GetOrders(
        [FromQuery] ProducerOrderItemQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetOrdersAsync(CurrentUserId, query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{orderItemId:guid}")]
    public async Task<ActionResult<ProducerOrderItemDto>> GetOrderItem(Guid orderItemId, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetOrderItemAsync(CurrentUserId, orderItemId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{orderItemId:guid}/accept")]
    public async Task<ActionResult<ProducerOrderItemDto>> Accept(Guid orderItemId, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.AcceptAsync(CurrentUserId, orderItemId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{orderItemId:guid}/reject")]
    public async Task<ActionResult<ProducerOrderItemDto>> Reject(
        Guid orderItemId, RejectOrderItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.RejectAsync(CurrentUserId, orderItemId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{orderItemId:guid}/processing")]
    public async Task<ActionResult<ProducerOrderItemDto>> StartProcessing(Guid orderItemId, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.StartProcessingAsync(CurrentUserId, orderItemId, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{orderItemId:guid}/ship")]
    public async Task<ActionResult<ProducerOrderItemDto>> Ship(
        Guid orderItemId, ShipOrderItemRequest request, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.ShipAsync(CurrentUserId, orderItemId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{orderItemId:guid}/deliver")]
    public async Task<ActionResult<ProducerOrderItemDto>> MarkDelivered(Guid orderItemId, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.MarkDeliveredAsync(CurrentUserId, orderItemId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("customers")]
    public async Task<ActionResult<List<ProducerCustomerDto>>> GetCustomers(CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetCustomersAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics/revenue")]
    public async Task<ActionResult<RevenueDashboardDto>> GetRevenueDashboard(
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetRevenueDashboardAsync(CurrentUserId, fromDate, toDate, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics/sales")]
    public async Task<ActionResult<SalesAnalyticsDto>> GetSalesAnalytics(
        [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] int topProductCount, CancellationToken cancellationToken)
    {
        var count = topProductCount <= 0 ? 5 : topProductCount;
        var result = await _producerOrderService.GetSalesAnalyticsAsync(CurrentUserId, fromDate, toDate, count, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics/visitors")]
    public async Task<ActionResult<VisitorAnalyticsDto>> GetVisitorAnalytics(CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetVisitorAnalyticsAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics/income-report")]
    public async Task<ActionResult<List<IncomeReportEntryDto>>> GetIncomeReport(
        [FromQuery] IncomeReportQueryParameters query, CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetIncomeReportAsync(CurrentUserId, query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("analytics/product-performance")]
    public async Task<ActionResult<List<ProductPerformanceDto>>> GetProductPerformance(CancellationToken cancellationToken)
    {
        var result = await _producerOrderService.GetProductPerformanceAsync(CurrentUserId, cancellationToken);
        return Ok(result);
    }
}
