using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Application.DTOs.Investment;

public class InvestmentMilestoneDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public InvestmentMilestoneStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}
