namespace ShilpoHubBD.Application.DTOs.Mentorship;

public class CreateMentorshipRequestRequest
{
    public Guid MentorProfileId { get; set; }
    public Guid? HeritageSkillId { get; set; }
    public string Message { get; set; } = string.Empty;
}
