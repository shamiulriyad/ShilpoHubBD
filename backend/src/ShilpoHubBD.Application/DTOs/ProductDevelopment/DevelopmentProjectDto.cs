using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Application.DTOs.ProductDevelopment;

public class DevelopmentProjectDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;

    public Guid BusinessPartnerId { get; set; }
    public string BusinessPartnerName { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string BusinessRequirements { get; set; } = string.Empty;
    public string ProductSpecifications { get; set; } = string.Empty;

    public DevelopmentStatus Status { get; set; }
    public DateTime? RespondedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Guid? FinalProductId { get; set; }
    public string? FinalProductName { get; set; }
    public string? FinalProductSlug { get; set; }

    public List<PrototypeVersionDto> PrototypeVersions { get; set; } = new();
    public List<DevelopmentCommentDto> Comments { get; set; } = new();
    public List<DevelopmentMilestoneDto> Milestones { get; set; } = new();
    public List<DevelopmentStatusEventDto> StatusHistory { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
