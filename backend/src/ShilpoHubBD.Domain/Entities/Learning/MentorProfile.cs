using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Learning;

public class MentorProfile
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string Bio { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public bool IsActive { get; set; } = true;
    public string ApprovalStatus { get; set; } = "Pending";
    public string ProofImageUrl { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTime? ReviewedAt { get; set; }

    public string? Location { get; set; }
    public string? AvailabilityNote { get; set; }
    public string? PreferredCategory { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Course> Courses { get; set; } = new List<Course>();
    public ICollection<MentorSkill> Skills { get; set; } = new List<MentorSkill>();
}
