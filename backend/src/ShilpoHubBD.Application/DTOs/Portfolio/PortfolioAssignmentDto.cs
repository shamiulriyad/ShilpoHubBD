namespace ShilpoHubBD.Application.DTOs.Portfolio;

public class PortfolioAssignmentDto
{
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public int? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime? GradedAt { get; set; }
}
