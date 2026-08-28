namespace ShilpoHubBD.Domain.Entities.ProductDevelopment;

public class ProductDevelopmentStatusEvent
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public ProductDevelopmentProject Project { get; set; } = null!;

    public DevelopmentStatus Status { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
}
