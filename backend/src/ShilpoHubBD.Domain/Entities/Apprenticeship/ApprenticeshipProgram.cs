using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Domain.Entities.Apprenticeship;

public class ApprenticeshipProgram
{
    public Guid Id { get; set; }

    // Exactly one of MentorId/TrainerProfileId is set, identifying the program provider.
    public Guid? MentorId { get; set; }
    public MentorProfile? Mentor { get; set; }

    public Guid? TrainerProfileId { get; set; }
    public AcademyMemberProfile? TrainerProfile { get; set; }

    public ProgramType Type { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public Guid? HeritageSkillId { get; set; }
    public HeritageSkill? HeritageSkill { get; set; }

    public string? Location { get; set; }
    public int? DurationWeeks { get; set; }
    public int? Capacity { get; set; }
    public string EligibilityRequirements { get; set; } = string.Empty;

    public ProgramStatus Status { get; set; } = ProgramStatus.Draft;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public ICollection<TrainingMilestone> Milestones { get; set; } = new List<TrainingMilestone>();
    public ICollection<ProgramApplication> Applications { get; set; } = new List<ProgramApplication>();
    public ICollection<ApprenticeEnrollment> Enrollments { get; set; } = new List<ApprenticeEnrollment>();
}
