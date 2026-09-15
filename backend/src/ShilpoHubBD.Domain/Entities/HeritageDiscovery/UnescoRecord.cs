using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Domain.Entities.HeritageDiscovery;

/// <summary>A UNESCO-recognised heritage site, tradition or archive relevant to Bangladesh.</summary>
public class UnescoRecord
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;
    public UnescoRecordType Type { get; set; }
    public string Description { get; set; } = string.Empty;
    public int InscribedYear { get; set; }

    /// <summary>Optional — intangible heritage (a craft, a song tradition) may not map to one district.</summary>
    public Guid? DistrictId { get; set; }
    public District? District { get; set; }

    public string? ImageUrl { get; set; }
    public string? OfficialUrl { get; set; }

    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
