using ShilpoHubBD.Application.DTOs.Learning;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAcademyMemberProfileService
{
    Task<AcademyMemberProfileDto> CreateProfileAsync(Guid userId, CreateAcademyMemberProfileRequest request, CancellationToken cancellationToken);
    Task<AcademyMemberProfileDto> UpdateProfileAsync(Guid userId, UpdateAcademyMemberProfileRequest request, CancellationToken cancellationToken);
    Task<AcademyMemberProfileDto> GetMyProfileAsync(Guid userId, CancellationToken cancellationToken);
    Task<AcademyMemberProfileDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<AcademyMemberProfileDto> AddSkillAsync(Guid userId, AddMemberSkillRequest request, CancellationToken cancellationToken);
    Task<AcademyMemberProfileDto> RemoveSkillAsync(Guid userId, Guid heritageSkillId, CancellationToken cancellationToken);

    Task<List<EnrollmentListItemDto>> GetMyLearningHistoryAsync(Guid userId, CancellationToken cancellationToken);
}
