namespace ShilpoHubBD.Domain.Entities.PerformingArts;

public enum PerformingArtSourceType
{
    OfficialInventory,
    Book,
    Article,
    Archive,
    Website,
    Other,
}

/// <summary>A citation backing a performing-arts record.</summary>
public class PerformingArtSource
{
    public Guid Id { get; set; }
    public Guid RecordId { get; set; }
    public PerformingArtRecord Record { get; set; } = null!;

    public PerformingArtSourceType SourceType { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public string? Url { get; set; }
    public DateTime? AccessedOn { get; set; }
    public string? Notes { get; set; }
    public int DisplayOrder { get; set; }
}
