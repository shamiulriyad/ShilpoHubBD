using ShilpoHubBD.Domain.Entities.Learning;

namespace ShilpoHubBD.Application.DTOs.Learning;

public class UpdateAcademyMemberProfileRequest
{
    public AcademyMemberRole Role { get; set; } = AcademyMemberRole.Learner;
    public string Bio { get; set; } = string.Empty;
    public string LearningPreferences { get; set; } = string.Empty;
}
