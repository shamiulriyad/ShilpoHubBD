namespace ShilpoHubBD.Domain.Entities.Cms;

/// <summary>A site-wide banner announcement, optionally scheduled to a window.</summary>
public class Announcement
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AnnouncementSeverity Severity { get; set; } = AnnouncementSeverity.Info;
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
