using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.ProductDevelopment;

public class ProductDevelopmentProject
{
    public Guid Id { get; set; }

    public Guid BusinessPartnerId { get; set; }
    public User BusinessPartner { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string BusinessRequirements { get; set; } = string.Empty;
    public string ProductSpecifications { get; set; } = string.Empty;

    public DevelopmentStatus Status { get; set; } = DevelopmentStatus.Requested;
    public DateTime? RespondedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    // Set once the project is converted into a real catalog Product (reuses the Marketplace module).
    public Guid? FinalProductId { get; set; }
    public Product? FinalProduct { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<PrototypeVersion> PrototypeVersions { get; set; } = new List<PrototypeVersion>();
    public ICollection<ProductDevelopmentComment> Comments { get; set; } = new List<ProductDevelopmentComment>();
    public ICollection<ProductDevelopmentMilestone> Milestones { get; set; } = new List<ProductDevelopmentMilestone>();
    public ICollection<ProductDevelopmentStatusEvent> StatusHistory { get; set; } = new List<ProductDevelopmentStatusEvent>();
}
