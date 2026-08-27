using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IFieldEvidenceService
{
    Task<PagedResult<FieldEvidenceDto>> GetForSurveyAsync(
        Guid userId, Guid surveyId, FieldEvidenceQueryParameters query, CancellationToken cancellationToken);

    Task<FieldEvidenceDto> GetByIdAsync(Guid userId, Guid surveyId, Guid evidenceId, CancellationToken cancellationToken);

    Task<FieldEvidenceDto> CreateAsync(
        Guid userId, Guid surveyId, CreateFieldEvidenceRequest request, CancellationToken cancellationToken);

    Task<FieldEvidenceDto> UpdateAsync(
        Guid userId, Guid surveyId, Guid evidenceId, UpdateFieldEvidenceRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid surveyId, Guid evidenceId, CancellationToken cancellationToken);
}
