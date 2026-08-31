namespace ShilpoHubBD.Application.DTOs.Research;

/// <summary>A single caller-supplied data point ("selected data") fed into an AI analysis.</summary>
public class ResearchDataPointDto
{
    public string Label { get; set; } = string.Empty;
    public string? Series { get; set; }
    public string? Category { get; set; }
    public double? NumericValue { get; set; }
    public DateTime? Timestamp { get; set; }
}
