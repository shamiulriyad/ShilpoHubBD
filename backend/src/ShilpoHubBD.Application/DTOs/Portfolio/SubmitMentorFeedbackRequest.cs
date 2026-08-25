namespace ShilpoHubBD.Application.DTOs.Portfolio;

public class SubmitMentorFeedbackRequest
{
    public Guid LearnerUserId { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? Rating { get; set; }
}
