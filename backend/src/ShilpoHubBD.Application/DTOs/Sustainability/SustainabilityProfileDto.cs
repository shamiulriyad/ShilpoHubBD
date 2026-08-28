namespace ShilpoHubBD.Application.DTOs.Sustainability;

public class SustainabilityProfileDto
{
    public Guid Id { get; set; }
    public Guid ProducerId { get; set; }
    public decimal EcoScore { get; set; }
    public string BadgeLevel { get; set; } = string.Empty;
    public decimal TotalCarbonSavingsKg { get; set; }
    public List<SustainableMaterialRecordDto> MaterialRecords { get; set; } = new();
    public List<SustainableMaterialCertificationDto> Certifications { get; set; } = new();
    public DateTime? LastCalculatedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
