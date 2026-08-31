namespace ShilpoHubBD.Application.DTOs.Mentorship;

public class MentorshipRequestListItemDto
{
    public Guid Id { get; set; }
    public Guid MentorProfileId { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public Guid LearnerUserId { get; set; }
    public string LearnerName { get; set; } = string.Empty;
    public string? HeritageSkillName { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
}
