namespace ShilpoHubBD.Application.DTOs.SupplierMatching;

public class SupplierMatchResultDto
{
    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;
    public string? WorkshopName { get; set; }
    public string? PrimaryCraft { get; set; }
    public string? DistrictName { get; set; }

    public decimal MatchScore { get; set; }
    public List<string> MatchReasons { get; set; } = new();

    public decimal AverageRating { get; set; }
    public int ProductCount { get; set; }
    public decimal MinPrice { get; set; }
    public int EstimatedProductionCapacity { get; set; }
    public int CertificationCount { get; set; }
    public bool IsHandmadeVerified { get; set; }
    public double? AverageDeliveryDays { get; set; }
}
