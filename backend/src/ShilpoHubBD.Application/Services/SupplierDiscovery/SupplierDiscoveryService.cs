using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.SupplierDiscovery;

public class SupplierDiscoveryService : ISupplierDiscoveryService
{
    private readonly ISupplierDiscoveryRepository _supplierDiscoveryRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProducerOrderService _producerOrderService;

    public SupplierDiscoveryService(
        ISupplierDiscoveryRepository supplierDiscoveryRepository, IUserRepository userRepository,
        IProductRepository productRepository, IProducerOrderService producerOrderService)
    {
        _supplierDiscoveryRepository = supplierDiscoveryRepository;
        _userRepository = userRepository;
        _productRepository = productRepository;
        _producerOrderService = producerOrderService;
    }

    public async Task<PagedResult<SupplierSearchResultDto>> SearchAsync(SupplierSearchParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _supplierDiscoveryRepository.SearchAsync(parameters, cancellationToken);

        return new PagedResult<SupplierSearchResultDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    public async Task<SupplierProfileDto> GetProducerProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        if (!producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        return await _supplierDiscoveryRepository.GetProducerProfileAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");
    }

    public async Task<ProducerBusinessProfileDto> GetBusinessProfileAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        if (!producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var craftProfile = await _supplierDiscoveryRepository.GetProducerProfileAsync(producerId, cancellationToken)
            ?? throw new NotFoundException("Producer not found.");

        var products = await _productRepository.GetByProducerAsync(producerId, cancellationToken);

        var now = DateTime.UtcNow;
        var revenueDashboard = await _producerOrderService.GetRevenueDashboardAsync(producerId, null, null, cancellationToken);
        var currentPeriod = await _producerOrderService.GetRevenueDashboardAsync(producerId, now.AddDays(-30), now, cancellationToken);
        var previousPeriod = await _producerOrderService.GetRevenueDashboardAsync(producerId, now.AddDays(-60), now.AddDays(-30), cancellationToken);
        var salesAnalytics = await _producerOrderService.GetSalesAnalyticsAsync(producerId, null, null, 5, cancellationToken);

        // Only counts are kept from the customer list — no name/email ever leaves this method.
        var customers = await _producerOrderService.GetCustomersAsync(producerId, cancellationToken);
        var totalCustomers = customers.Count;
        var repeatCustomers = customers.Count(c => c.TotalOrders > 1);

        decimal? salesGrowthPercent = previousPeriod.TotalRevenue == 0
            ? (currentPeriod.TotalRevenue == 0 ? null : 100m)
            : Math.Round((currentPeriod.TotalRevenue - previousPeriod.TotalRevenue) / previousPeriod.TotalRevenue * 100, 2);

        return new ProducerBusinessProfileDto
        {
            ProducerId = producerId,
            ProducerName = craftProfile.ProducerName,
            WorkshopName = craftProfile.WorkshopName,
            PrimaryCraft = craftProfile.PrimaryCraft,
            DistrictName = craftProfile.DistrictName,
            HeritageVerificationStatus = craftProfile.HeritageVerificationStatus,

            TotalProductCount = products.Count,
            ActiveProductCount = products.Count(p => p.IsActive && p.ApprovalStatus == ProductApprovalStatus.Approved),

            AverageRating = craftProfile.AverageRating,
            TotalReviewCount = craftProfile.TotalReviewCount,

            TotalRevenue = revenueDashboard.TotalRevenue,
            TotalOrders = revenueDashboard.TotalOrders,
            TotalItemsSold = revenueDashboard.TotalItemsSold,
            AverageOrderValue = revenueDashboard.AverageOrderValue,
            SalesGrowthPercent = salesGrowthPercent,

            BestSellingProducts = salesAnalytics.TopProducts,

            TotalCustomerCount = totalCustomers,
            RepeatCustomerCount = repeatCustomers,
            RepeatCustomerRatePercent = totalCustomers == 0 ? null : Math.Round(repeatCustomers * 100m / totalCustomers, 2),

            CertificationCount = craftProfile.Certifications.Count,
            EstimatedProductionCapacity = craftProfile.EstimatedProductionCapacity,
        };
    }
}
