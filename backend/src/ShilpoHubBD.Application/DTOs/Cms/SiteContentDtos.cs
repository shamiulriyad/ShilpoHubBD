namespace ShilpoHubBD.Application.DTOs.Cms;

public class SiteContentItemDto
{
    public Guid Id { get; set; }
    public string Group { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Body { get; set; }
    public string? LinkUrl { get; set; }
    public string? Extra { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>Used for both create and update (IsActive is ignored on create).</summary>
public class SaveSiteContentItemRequest
{
    public string Group { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? Body { get; set; }
    public string? LinkUrl { get; set; }
    public string? Extra { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
