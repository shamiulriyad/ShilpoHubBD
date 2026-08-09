using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Sustainability;

namespace ShilpoHubBD.Application.DTOs.SupplierDiscovery;

public class SupplierProfileDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string ProducerEmail { get; set; } = string.Empty;
    public DateTime MemberSince { get; set; }

    public string? WorkshopName { get; set; }
    public string? WorkshopDescription { get; set; }
    public string? PrimaryCraft { get; set; }
    public int? YearsOfExperience { get; set; }
    public int? EstablishedYear { get; set; }
    public string? DistrictName { get; set; }
    public HeritageVerificationStatus? HeritageVerificationStatus { get; set; }
    public int? LegacyScore { get; set; }

    public decimal? EcoScore { get; set; }
    public GreenBadgeLevel? BadgeLevel { get; set; }
    public List<string> Materials { get; set; } = new();

    public List<SupplierCertificationDto> Certifications { get; set; } = new();

    public int ProductCount { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviewCount { get; set; }
    public int TotalSalesCount { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    // Approximated from aggregate active-product stock; see SupplierSearchResultDto.
    public int EstimatedProductionCapacity { get; set; }

    public List<SupplierProductSummaryDto> Products { get; set; } = new();
}
