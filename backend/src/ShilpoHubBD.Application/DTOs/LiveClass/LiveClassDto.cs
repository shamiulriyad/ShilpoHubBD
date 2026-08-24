namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class LiveClassDto
{
    public Guid Id { get; set; }
    public Guid InstructorUserId { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public Guid? CourseId { get; set; }
    public string? CourseTitle { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MeetingUrl { get; set; }
    public int? MaxParticipants { get; set; }
    public int ParticipantCount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime ScheduledStartAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<LiveClassParticipantDto> Participants { get; set; } = new();
    public List<LiveClassQuestionDto> Questions { get; set; } = new();
}
