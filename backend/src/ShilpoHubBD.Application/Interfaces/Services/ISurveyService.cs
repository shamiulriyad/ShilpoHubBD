using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.FieldResearch;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ISurveyService
{
    Task<PagedResult<SurveyListItemDto>> GetForUserAsync(
        Guid userId, SurveyQueryParameters query, CancellationToken cancellationToken);

    Task<SurveyDetailDto> GetByIdAsync(Guid userId, Guid surveyId, CancellationToken cancellationToken);

    Task<SurveyDetailDto> CreateAsync(Guid userId, CreateSurveyRequest request, CancellationToken cancellationToken);

    Task<SurveyDetailDto> UpdateAsync(Guid userId, Guid surveyId, UpdateSurveyRequest request, CancellationToken cancellationToken);

    Task<SurveyDetailDto> UpdateStatusAsync(
        Guid userId, Guid surveyId, UpdateSurveyStatusRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid surveyId, CancellationToken cancellationToken);

    Task<SurveyQuestionDto> AddQuestionAsync(
        Guid userId, Guid surveyId, CreateSurveyQuestionRequest request, CancellationToken cancellationToken);

    Task<SurveyQuestionDto> UpdateQuestionAsync(
        Guid userId, Guid surveyId, Guid questionId, UpdateSurveyQuestionRequest request, CancellationToken cancellationToken);

    Task DeleteQuestionAsync(Guid userId, Guid surveyId, Guid questionId, CancellationToken cancellationToken);

    Task<List<SurveyFieldAssignmentDto>> GetFieldResearchersAsync(Guid userId, Guid surveyId, CancellationToken cancellationToken);

    Task<SurveyFieldAssignmentDto> AssignFieldResearcherAsync(
        Guid userId, Guid surveyId, AssignFieldResearcherRequest request, CancellationToken cancellationToken);

    Task<SurveyFieldAssignmentDto> UpdateFieldAssignmentAsync(
        Guid userId, Guid surveyId, Guid assignmentId, UpdateFieldAssignmentRequest request, CancellationToken cancellationToken);

    Task RemoveFieldResearcherAsync(Guid userId, Guid surveyId, Guid assignmentId, CancellationToken cancellationToken);

    Task<List<DataCollectionEventDto>> GetHistoryAsync(Guid userId, Guid surveyId, int take, CancellationToken cancellationToken);
}
