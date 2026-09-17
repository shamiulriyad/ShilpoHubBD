using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.DTOs.Cms;

public class UpdateAnnouncementRequest
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AnnouncementSeverity Severity { get; set; }
    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public bool IsActive { get; set; }
}
