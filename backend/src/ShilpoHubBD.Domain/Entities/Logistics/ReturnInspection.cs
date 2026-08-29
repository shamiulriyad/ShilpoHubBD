using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Logistics;

/// <summary>A recorded inspection pass over the goods received for a <see cref="ReturnRequest"/>.</summary>
public class ReturnInspection
{
    public Guid Id { get; set; }

    public Guid ReturnRequestId { get; set; }
    public ReturnRequest ReturnRequest { get; set; } = null!;

    public Guid? InspectedByUserId { get; set; }
    public User? InspectedBy { get; set; }

    public DateTime InspectedAt { get; set; }

    public ReturnItemCondition OverallCondition { get; set; }
    public string Summary { get; set; } = string.Empty;
    public ReturnResolutionType RecommendedResolution { get; set; }

    /// <summary>Optional JSON array of evidence photo URLs.</summary>
    public string? PhotosJson { get; set; }

    public DateTime CreatedAt { get; set; }
}
