namespace ShilpoHubBD.Domain.Entities.ProductDevelopment;

public class ProductDevelopmentMilestone
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public ProductDevelopmentProject Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime DueDate { get; set; }
    public DevelopmentMilestoneStatus Status { get; set; } = DevelopmentMilestoneStatus.Pending;
    public DateTime? CompletedAt { get; set; }
    public int DisplayOrder { get; set; }
}
