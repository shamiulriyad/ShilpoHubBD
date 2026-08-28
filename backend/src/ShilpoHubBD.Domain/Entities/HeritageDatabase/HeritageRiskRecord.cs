using ShilpoHubBD.Domain.Entities.Community;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.HeritageDatabase;

/// <summary>
/// A heritage-at-risk assessment record (endangered crafts, aging artisans, material scarcity, ...).
/// Optionally linked to an existing District, Village or Producer for cross-referencing.
/// </summary>
public class HeritageRiskRecord
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public HeritageRiskCategory Category { get; set; } = HeritageRiskCategory.SkillLoss;
    public HeritageRiskLevel Level { get; set; } = HeritageRiskLevel.Moderate;

    public string? CraftName { get; set; }

    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public Guid? VillageId { get; set; }
    public Village? Village { get; set; }

    public Guid? ProducerId { get; set; }
    public User? Producer { get; set; }

    public int? AffectedArtisanCount { get; set; }
    public string? ContributingFactors { get; set; }
    public string? RecommendedActions { get; set; }
    public string? Source { get; set; }
    public int? AssessmentYear { get; set; }
    public DateTime? AssessedOn { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
