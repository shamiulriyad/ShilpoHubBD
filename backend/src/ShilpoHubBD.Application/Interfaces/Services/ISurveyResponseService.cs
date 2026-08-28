using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISurveyResponseService
{
    Task<PagedResult<SurveyResponseListItemDto>> GetForSurveyAsync(
        Guid userId, Guid surveyId, SurveyResponseQueryParameters query, CancellationToken cancellationToken);

    Task<SurveyResponseDto> GetByIdAsync(Guid userId, Guid surveyId, Guid responseId, CancellationToken cancellationToken);

    Task<SurveyResponseDto> CreateAsync(
        Guid userId, Guid surveyId, CreateSurveyResponseRequest request, CancellationToken cancellationToken);

    Task<SurveyResponseDto> UpdateAsync(
        Guid userId, Guid surveyId, Guid responseId, UpdateSurveyResponseRequest request, CancellationToken cancellationToken);

    Task<SurveyResponseDto> SubmitAsync(Guid userId, Guid surveyId, Guid responseId, CancellationToken cancellationToken);

    Task<SurveyResponseDto> ReviewAsync(
        Guid userId, Guid surveyId, Guid responseId, ReviewSurveyResponseRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid surveyId, Guid responseId, CancellationToken cancellationToken);
}
