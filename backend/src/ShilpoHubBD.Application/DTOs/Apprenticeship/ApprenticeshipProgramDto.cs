namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ApprenticeshipProgramDto
{
    public Guid Id { get; set; }
    public Guid? MentorId { get; set; }
    public Guid? TrainerProfileId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? HeritageSkillId { get; set; }
    public string? HeritageSkillName { get; set; }
    public string? Location { get; set; }
    public int? DurationWeeks { get; set; }
    public int? Capacity { get; set; }
    public string EligibilityRequirements { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int ActiveEnrollmentCount { get; set; }
    public List<TrainingMilestoneDto> Milestones { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
}
