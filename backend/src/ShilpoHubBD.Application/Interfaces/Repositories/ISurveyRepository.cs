using ShilpoHubBD.Application.DTOs.FieldResearch;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>Aggregate repository for the Survey &amp; Field Data Collection module.</summary>
public interface ISurveyRepository
{
    // Surveys
    Task<Survey?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Survey?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task<(List<Survey> Items, int TotalCount)> GetPagedForUserAsync(
        Guid userId, SurveyQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(Survey survey, CancellationToken cancellationToken);
    void Remove(Survey survey);

    // Questions
    Task<SurveyQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task<List<SurveyQuestion>> GetQuestionsAsync(Guid surveyId, CancellationToken cancellationToken);
    Task<bool> QuestionHasAnswersAsync(Guid questionId, CancellationToken cancellationToken);
    Task AddQuestionAsync(SurveyQuestion question, CancellationToken cancellationToken);
    void RemoveQuestion(SurveyQuestion question);

    // Field assignments
    Task<SurveyFieldAssignment?> GetAssignmentAsync(Guid surveyId, Guid userId, CancellationToken cancellationToken);
    Task<SurveyFieldAssignment?> GetAssignmentByIdAsync(Guid assignmentId, CancellationToken cancellationToken);
    Task<List<SurveyFieldAssignment>> GetAssignmentsAsync(Guid surveyId, CancellationToken cancellationToken);
    Task AddAssignmentAsync(SurveyFieldAssignment assignment, CancellationToken cancellationToken);
    void RemoveAssignment(SurveyFieldAssignment assignment);

    // Responses
    Task<SurveyResponse?> GetResponseByIdAsync(Guid responseId, CancellationToken cancellationToken);
    Task<(List<SurveyResponse> Items, int TotalCount)> GetResponsesAsync(
        Guid surveyId, SurveyResponseQueryParameters query, CancellationToken cancellationToken);
    Task AddResponseAsync(SurveyResponse response, CancellationToken cancellationToken);
    void RemoveResponse(SurveyResponse response);
    void RemoveAnswers(IEnumerable<SurveyResponseAnswer> answers);

    // Evidence
    Task<FieldEvidence?> GetEvidenceByIdAsync(Guid evidenceId, CancellationToken cancellationToken);
    Task<(List<FieldEvidence> Items, int TotalCount)> GetEvidenceAsync(
        Guid surveyId, FieldEvidenceQueryParameters query, CancellationToken cancellationToken);
    Task AddEvidenceAsync(FieldEvidence evidence, CancellationToken cancellationToken);
    void RemoveEvidence(FieldEvidence evidence);

    // History
    Task AddEventAsync(DataCollectionEvent collectionEvent, CancellationToken cancellationToken);
    Task<List<DataCollectionEvent>> GetEventsAsync(Guid surveyId, int take, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
