using ShilpoHubBD.Application.DTOs.Achievement;
using ShilpoHubBD.Application.DTOs.Apprenticeship;
using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.DTOs.Portfolio;

public class PortfolioDto
{
    public Guid Id { get; set; }
    public Guid AcademyMemberProfileId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Headline { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;

    public List<AcademyMemberSkillDto> HeritageSkills { get; set; } = new();
    public List<EnrollmentListItemDto> CompletedCourses { get; set; } = new();
    public List<TrainingCertificateDto> Certificates { get; set; } = new();
    public List<PortfolioProjectDto> Projects { get; set; } = new();
    public List<PortfolioAssignmentDto> Assignments { get; set; } = new();
    public List<UserAchievementDto> Achievements { get; set; } = new();
    public List<ApprenticeEnrollmentListItemDto> ApprenticeshipExperience { get; set; } = new();
    public List<MentorFeedbackDto> MentorFeedback { get; set; } = new();

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
