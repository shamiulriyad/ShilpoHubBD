using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.MentorMatching;

// Raw per-mentor signals assembled by the repository; MentorMatchingService turns these into a
// scored MentorMatchResultDto. Not exposed directly through the controller.
public class MentorMatchCandidateDto
{
    public Guid MentorProfileId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public string? Location { get; set; }
    public string? AvailabilityNote { get; set; }
    public string? PreferredCategory { get; set; }

    public bool HasMatchingSkill { get; set; }
    public SkillLevel? MatchingSkillLevel { get; set; }
    public bool HasMatchingLocation { get; set; }
    public bool HasMatchingGoalKeyword { get; set; }
    public bool HasMatchingAvailability { get; set; }
    public bool HasMatchingCategory { get; set; }
}
