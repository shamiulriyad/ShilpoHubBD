using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IQuizService
{
    Task<QuizDto> CreateAsync(Guid userId, Guid courseId, CreateQuizRequest request, CancellationToken cancellationToken);

    Task<QuizDto> UpdateAsync(Guid userId, Guid quizId, UpdateQuizRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid quizId, CancellationToken cancellationToken);

    Task<QuizDto> GetByIdAsync(Guid userId, Guid quizId, CancellationToken cancellationToken);

    Task<List<QuizListItemDto>> GetByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);

    Task<QuizQuestionDto> AddQuestionAsync(Guid userId, Guid quizId, CreateQuizQuestionRequest request, CancellationToken cancellationToken);

    Task<QuizQuestionDto> UpdateQuestionAsync(
        Guid userId, Guid quizId, Guid questionId, UpdateQuizQuestionRequest request, CancellationToken cancellationToken);

    Task DeleteQuestionAsync(Guid userId, Guid quizId, Guid questionId, CancellationToken cancellationToken);

    Task<QuizAttemptStartDto> StartAttemptAsync(Guid studentUserId, Guid quizId, CancellationToken cancellationToken);

    Task<QuizAttemptResultDto> SubmitAttemptAsync(
        Guid studentUserId, Guid attemptId, SubmitQuizAttemptRequest request, CancellationToken cancellationToken);

    Task<QuizAttemptResultDto> GetAttemptResultAsync(Guid userId, Guid attemptId, CancellationToken cancellationToken);

    Task<List<QuizAttemptListItemDto>> GetMyAttemptsAsync(Guid studentUserId, Guid quizId, CancellationToken cancellationToken);

    Task<List<QuizAttemptListItemDto>> GetAttemptsForTrainerAsync(Guid userId, Guid quizId, CancellationToken cancellationToken);
}
