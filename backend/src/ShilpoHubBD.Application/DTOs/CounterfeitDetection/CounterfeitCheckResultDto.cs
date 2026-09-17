namespace ShilpoHubBD.Application.DTOs.CounterfeitDetection;

public class CounterfeitCheckResultDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal RiskScore { get; set; }
    public string RiskLevel { get; set; } = string.Empty;
    public List<string> Signals { get; set; } = new();
}
