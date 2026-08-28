namespace ShilpoHubBD.Application.DTOs.Portfolio;

public class MentorFeedbackDto
{
    public Guid Id { get; set; }
    public Guid MentorProfileId { get; set; }
    public string MentorName { get; set; } = string.Empty;
    public Guid LearnerUserId { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public string? HeritageSkillName { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? Rating { get; set; }
    public DateTime CreatedAt { get; set; }
}
