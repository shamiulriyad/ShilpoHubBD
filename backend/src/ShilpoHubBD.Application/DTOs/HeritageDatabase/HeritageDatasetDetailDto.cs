using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string AccessLevel { get; set; } = string.Empty;
    public string SourceType { get; set; } = string.Empty;
    public string? SourceOrganization { get; set; }
    public string? SourceReference { get; set; }
    public string? License { get; set; }
    public string? Tags { get; set; }
    public bool IsLive { get; set; }
    public int RecordCount { get; set; }
    public int VersionCount { get; set; }
    public Guid? CurrentVersionId { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public string MyAccess { get; set; } = string.Empty;
    public bool CanManage { get; set; }
    public DateTime? DataUpdatedAt { get; set; }
    public DateTime? LastRefreshedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<HeritageDatasetVersionDto> Versions { get; set; } = new();
    public List<HeritageDatasetAccessGrantDto> AccessGrants { get; set; } = new();
}
