using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.LiveClass;

public class LiveClass
{
    public Guid Id { get; set; }

    public Guid InstructorUserId { get; set; }
    public User Instructor { get; set; } = null!;

    public Guid? CourseId { get; set; }
    public Course? Course { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? MeetingUrl { get; set; }
    public int? MaxParticipants { get; set; }

    public LiveClassStatus Status { get; set; } = LiveClassStatus.Scheduled;

    public DateTime ScheduledStartAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<LiveClassParticipant> Participants { get; set; } = new List<LiveClassParticipant>();
    public ICollection<LiveClassQuestion> Questions { get; set; } = new List<LiveClassQuestion>();
    public ICollection<LiveClassAttendance> Attendances { get; set; } = new List<LiveClassAttendance>();
}
