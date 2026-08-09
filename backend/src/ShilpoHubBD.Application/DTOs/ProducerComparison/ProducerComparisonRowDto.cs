using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Application.DTOs.ProducerComparison;

public class ProducerComparisonRowDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string? WorkshopName { get; set; }
    public string? PrimaryCraft { get; set; }

    // Location
    public string? DistrictName { get; set; }

    // Experience
    public int? YearsOfExperience { get; set; }
    public int? EstablishedYear { get; set; }
    public HeritageVerificationStatus? HeritageVerificationStatus { get; set; }

    // Price
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? AveragePrice { get; set; }

    // Rating / Product quality
    public decimal AverageRating { get; set; }
    public int TotalReviewCount { get; set; }
    public int ProductCount { get; set; }
    public int HandmadeVerifiedProductCount { get; set; }
    public decimal HandmadeVerifiedRatio { get; set; }

    // Production capacity (approximated from aggregate active-product stock; see SupplierSearchResultDto)
    public int EstimatedProductionCapacity { get; set; }

    // Certifications
    public int CertificationCount { get; set; }
    public List<SupplierCertificationDto> Certifications { get; set; } = new();

    // Delivery performance
    public double? AverageDeliveryDays { get; set; }

    // Previous orders / track record
    public int TotalOrdersFulfilled { get; set; }
    public int TotalUnitsSold { get; set; }
}
