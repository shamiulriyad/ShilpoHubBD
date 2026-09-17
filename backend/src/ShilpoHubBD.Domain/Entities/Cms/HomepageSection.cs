namespace ShilpoHubBD.Domain.Entities.Cms;

/// <summary>An editable content block on the public homepage (hero banner, featured strip, promo tile), ordered for display.</summary>
public class HomepageSection
{
    public Guid Id { get; set; }
    public string SectionKey { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Subtitle { get; set; }
    public string? ImageUrl { get; set; }
    public string? LinkUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
