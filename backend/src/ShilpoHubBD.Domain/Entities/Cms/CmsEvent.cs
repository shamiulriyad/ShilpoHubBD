namespace ShilpoHubBD.Domain.Entities.Cms;

/// <summary>A CMS-authored announcement/listing page for an event (distinct from <c>HeritageDiscovery.HeritageFestival</c> or <c>Community.CulturalEvent</c>).</summary>
public class CmsEvent
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Location { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public bool IsPublished { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
