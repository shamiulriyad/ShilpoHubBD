namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class ImpactRecordDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public DateTime RecordedAt { get; set; }
}
