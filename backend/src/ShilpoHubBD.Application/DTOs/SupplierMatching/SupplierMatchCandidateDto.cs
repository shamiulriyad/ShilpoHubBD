namespace ShilpoHubBD.Application.DTOs.SupplierMatching;

// Raw per-producer signals assembled by the repository; SupplierMatchingService turns these into
// a scored SupplierMatchResultDto. Not exposed directly through the controller.
public class SupplierMatchCandidateDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string? WorkshopName { get; set; }
    public string? PrimaryCraft { get; set; }
    public string? DistrictName { get; set; }

    public int ProductCount { get; set; }
    public decimal MinPrice { get; set; }
    public int EstimatedProductionCapacity { get; set; }
    public decimal AverageRating { get; set; }
    public int TotalReviewCount { get; set; }
    public bool IsHandmadeVerified { get; set; }
    public int CertificationCount { get; set; }
    public double? AverageDeliveryDays { get; set; }

    public bool HasMatchingCategory { get; set; }
    public bool HasMatchingDistrict { get; set; }
    public bool HasMatchingKeyword { get; set; }
    public bool HasMatchingMaterial { get; set; }
    public bool HasProductWithinBudget { get; set; }
}
