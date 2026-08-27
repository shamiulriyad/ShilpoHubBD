namespace ShilpoHubBD.Application.DTOs.Research;

public class ResearchAIFindingDto
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Heading { get; set; } = string.Empty;
    public string Detail { get; set; } = string.Empty;
    public string? Metric { get; set; }
    public double? Score { get; set; }
    public int DisplayOrder { get; set; }
}
