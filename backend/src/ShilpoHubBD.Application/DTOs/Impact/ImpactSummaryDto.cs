namespace ShilpoHubBD.Application.DTOs.Impact;

public class ImpactSummaryDto
{
    public int HeritageScore { get; set; }
    public int FamiliesSupported { get; set; }
    public int DistinctDistrictsSupported { get; set; }
    public int DistinctCategoriesSupported { get; set; }
    public int TotalItemsPurchased { get; set; }

    // Rough estimate only: assumes a fixed CO2 saving per handmade/local item versus a
    // mass-produced, long-shipped equivalent. Not based on per-product life-cycle data.
    public decimal EstimatedCo2SavingsKg { get; set; }
}
