using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.MentorMatching;

public class MentorMatchRequest
{
    public Guid? HeritageSkillId { get; set; }
    public SkillLevel? MinSkillLevel { get; set; }
    public string? LearningGoalKeyword { get; set; }
    public string? Location { get; set; }
    public int? MinYearsOfExperience { get; set; }
    public string? AvailabilityKeyword { get; set; }
    public string? PreferredCategory { get; set; }
    public int MaxResults { get; set; } = 10;
}
