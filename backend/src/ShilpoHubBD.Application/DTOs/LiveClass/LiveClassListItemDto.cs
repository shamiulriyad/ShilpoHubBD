namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class LiveClassListItemDto
{
    public Guid Id { get; set; }
    public string InstructorName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public int? MaxParticipants { get; set; }
    public int ParticipantCount { get; set; }
    public DateTime ScheduledStartAt { get; set; }
}
