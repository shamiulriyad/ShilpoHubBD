namespace ShilpoHubBD.Application.DTOs.Mentorship;

public class MentorshipRequestDto
{
    public Guid Id { get; set; }
    public Guid MentorProfileId { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public Guid LearnerUserId { get; set; }
    public string LearnerName { get; set; } = string.Empty;
    public Guid? HeritageSkillId { get; set; }
    public string? HeritageSkillName { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}
