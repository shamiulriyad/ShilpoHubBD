namespace ShilpoHubBD.Application.DTOs.Learning;

public class MentorProfileDto
{
    public string ApprovalStatus { get; set; } = "Pending";
    public string ProofImageUrl { get; set; } = string.Empty;
    public string? ReviewNote { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Expertise { get; set; } = string.Empty;
    public int YearsOfExperience { get; set; }
    public bool IsActive { get; set; }
    public string? Location { get; set; }
    public string? AvailabilityNote { get; set; }
    public string? PreferredCategory { get; set; }
    public List<MentorSkillDto> Skills { get; set; } = new();
    public int PublishedCourseCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
