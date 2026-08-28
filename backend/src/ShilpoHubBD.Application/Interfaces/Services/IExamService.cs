using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IExamService
{
    Task<ExamDto> CreateAsync(Guid userId, Guid courseId, CreateExamRequest request, CancellationToken cancellationToken);

    Task<ExamDto> UpdateAsync(Guid userId, Guid examId, UpdateExamRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid examId, CancellationToken cancellationToken);

    Task<ExamDto> GetByIdAsync(Guid userId, Guid examId, CancellationToken cancellationToken);

    Task<List<ExamListItemDto>> GetByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);

    Task<ExamQuestionDto> AddQuestionAsync(Guid userId, Guid examId, CreateExamQuestionRequest request, CancellationToken cancellationToken);

    Task<ExamQuestionDto> UpdateQuestionAsync(
        Guid userId, Guid examId, Guid questionId, UpdateExamQuestionRequest request, CancellationToken cancellationToken);

    Task DeleteQuestionAsync(Guid userId, Guid examId, Guid questionId, CancellationToken cancellationToken);

    Task<ExamAttemptStartDto> StartAttemptAsync(Guid studentUserId, Guid examId, CancellationToken cancellationToken);

    Task<ExamAttemptResultDto> SubmitAttemptAsync(
        Guid studentUserId, Guid attemptId, SubmitExamAttemptRequest request, CancellationToken cancellationToken);

    Task<ExamAttemptResultDto> GetAttemptResultAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken);

    Task<List<ExamAttemptListItemDto>> GetMyAttemptsAsync(Guid studentUserId, Guid examId, CancellationToken cancellationToken);

    Task<List<ExamAttemptListItemDto>> GetAttemptsForTrainerAsync(Guid userId, Guid examId, CancellationToken cancellationToken);

    Task<ExamAttemptResultDto> EvaluateAnswerAsync(
        Guid userId, Guid attemptId, Guid questionId, EvaluateExamAnswerRequest request, CancellationToken cancellationToken);

    Task<ExamAttemptResultDto> FinalizeEvaluationAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken);
}
