namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class CreateLiveClassRequest
{
    public Guid? CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MeetingUrl { get; set; }
    public int? MaxParticipants { get; set; }
    public DateTime ScheduledStartAt { get; set; }
}
