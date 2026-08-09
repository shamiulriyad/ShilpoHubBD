using ShilpoHubBD.Domain.Entities.HeritageIdentity;

namespace ShilpoHubBD.Application.DTOs.SupplierDiscovery;

public class SupplierSearchResultDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string? WorkshopName { get; set; }
    public string? PrimaryCraft { get; set; }
    public int? YearsOfExperience { get; set; }

    public Guid? DistrictId { get; set; }
    public string? DistrictName { get; set; }

    public HeritageVerificationStatus? HeritageVerificationStatus { get; set; }
    public bool IsHandmadeVerified { get; set; }

    public decimal AverageRating { get; set; }
    public int TotalReviewCount { get; set; }
    public int ProductCount { get; set; }
    public decimal MinPrice { get; set; }
    public decimal MaxPrice { get; set; }

    // Approximated from aggregate active-product stock, since no dedicated
    // production-capacity figure exists in the schema yet.
    public int EstimatedProductionCapacity { get; set; }

    public int CertificationCount { get; set; }
    public decimal? EcoScore { get; set; }
    public int? LegacyScore { get; set; }
}
