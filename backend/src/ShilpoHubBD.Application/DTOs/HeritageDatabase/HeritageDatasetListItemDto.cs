namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetListItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public bool IsLive { get; set; }
    public int RecordCount { get; set; }
    public int VersionCount { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public DateTime? DataUpdatedAt { get; set; }
    public DateTime? LastRefreshedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
