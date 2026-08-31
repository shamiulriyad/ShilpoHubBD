using ShilpoHubBD.Application.DTOs.SkillAssessment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISkillAssessmentService
{
    Task<SkillAssessmentResultDto> RunAssessmentAsync(Guid userId, Guid heritageSkillId, CancellationToken cancellationToken);

    Task<SkillAssessmentResultDto> GetByIdAsync(Guid userId, Guid assessmentId, CancellationToken cancellationToken);

    Task<List<SkillAssessmentListItemDto>> GetHistoryAsync(Guid userId, CancellationToken cancellationToken);
}
