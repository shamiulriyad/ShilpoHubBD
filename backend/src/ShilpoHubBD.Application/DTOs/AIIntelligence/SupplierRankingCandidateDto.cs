namespace ShilpoHubBD.Application.DTOs.AIIntelligence;

// Pre-fetched signals for one candidate producer, assembled by the service from existing
// Product/Order/Review/Certification data before being handed to the AI provider.
public class SupplierRankingCandidateDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public int ProductCount { get; set; }
    public int EstimatedProductionCapacity { get; set; }
    public int CertificationCount { get; set; }
    public bool IsHandmadeVerified { get; set; }
}
