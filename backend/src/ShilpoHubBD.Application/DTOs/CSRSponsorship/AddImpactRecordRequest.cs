namespace ShilpoHubBD.Application.DTOs.CSRSponsorship;

public class AddImpactRecordRequest
{
    public string Description { get; set; } = string.Empty;
    public string Metric { get; set; } = string.Empty;
    public decimal Value { get; set; }
}
