using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class DevelopmentMilestoneDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DevelopmentMilestoneStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}
