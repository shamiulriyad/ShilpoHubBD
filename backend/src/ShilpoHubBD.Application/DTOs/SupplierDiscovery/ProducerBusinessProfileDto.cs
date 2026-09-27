using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Application.DTOs.SupplierDiscovery;

/// <summary>
/// Aggregated, partnership-evaluation view of a producer's commercial performance for a Business
/// Partner. Every field is a business-level aggregate — no customer names, emails, phone numbers
/// or order-level detail are ever included here.
/// </summary>
public class ProducerBusinessProfileDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string? WorkshopName { get; set; }
    public string? PrimaryCraft { get; set; }
    public string? DistrictName { get; set; }
    public HeritageVerificationStatus? HeritageVerificationStatus { get; set; }

    public int TotalProductCount { get; set; }
    public int ActiveProductCount { get; set; }

    public decimal AverageRating { get; set; }
    public int TotalReviewCount { get; set; }

    public decimal TotalRevenue { get; set; }
    public int TotalOrders { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal AverageOrderValue { get; set; }

    /// <summary>Revenue change over the last 30 days vs. the prior 30 days. Null when there's no revenue in the prior window to compare against.</summary>
    public decimal? SalesGrowthPercent { get; set; }

    public List<ProductSalesDto> BestSellingProducts { get; set; } = new();

    public int TotalCustomerCount { get; set; }
    public int RepeatCustomerCount { get; set; }
    public decimal? RepeatCustomerRatePercent { get; set; }

    public int CertificationCount { get; set; }
    public int EstimatedProductionCapacity { get; set; }
}
