using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerOrderService
{
    Task<PagedResult<ProducerOrderItemDto>> GetOrdersAsync(
        Guid producerId, ProducerOrderItemQueryParameters query, CancellationToken cancellationToken);

    Task<ProducerOrderItemDto> GetOrderItemAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken);

    Task<ProducerOrderItemDto> AcceptAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken);

    Task<ProducerOrderItemDto> RejectAsync(
        Guid producerId, Guid orderItemId, RejectOrderItemRequest request, CancellationToken cancellationToken);

    Task<ProducerOrderItemDto> StartProcessingAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken);

    Task<ProducerOrderItemDto> ShipAsync(
        Guid producerId, Guid orderItemId, ShipOrderItemRequest request, CancellationToken cancellationToken);

    Task<ProducerOrderItemDto> MarkDeliveredAsync(Guid producerId, Guid orderItemId, CancellationToken cancellationToken);

    Task<List<ProducerCustomerDto>> GetCustomersAsync(Guid producerId, CancellationToken cancellationToken);

    Task<RevenueDashboardDto> GetRevenueDashboardAsync(
        Guid producerId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken);

    Task<SalesAnalyticsDto> GetSalesAnalyticsAsync(
        Guid producerId, DateTime? fromDate, DateTime? toDate, int topProductCount, CancellationToken cancellationToken);

    Task<VisitorAnalyticsDto> GetVisitorAnalyticsAsync(Guid producerId, CancellationToken cancellationToken);

    Task<List<IncomeReportEntryDto>> GetIncomeReportAsync(
        Guid producerId, IncomeReportQueryParameters query, CancellationToken cancellationToken);

    Task<List<ProductPerformanceDto>> GetProductPerformanceAsync(Guid producerId, CancellationToken cancellationToken);
}
