namespace ShilpoHubBD.Domain.Entities.PerformingArts;

public enum PerformingArtMediaType
{
    Image,
    Audio,
    Video,
}

/// <summary>An image, audio or video item for a performing-arts record. Credit is mandatory.</summary>
public class PerformingArtMedia
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public PerformingArtRecord Record { get; set; } = null!;

    public PerformingArtMediaType MediaType { get; set; }

    /// <summary>An https:// URL or a platform upload path (/uploads/...).</summary>
    public string Url { get; set; } = string.Empty;
    public string? CaptionEn { get; set; }
    public string? CaptionBn { get; set; }
    public string Credit { get; set; } = string.Empty;
    public string? License { get; set; }
    public string? SourceUrl { get; set; }
    public int DisplayOrder { get; set; }
}
