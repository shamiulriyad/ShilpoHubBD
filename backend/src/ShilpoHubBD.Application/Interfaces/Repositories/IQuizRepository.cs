using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IQuizRepository
{
    Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Quiz>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task AddAsync(Quiz quiz, CancellationToken cancellationToken);
    void Remove(Quiz quiz);

    Task<QuizQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken);
    void RemoveQuestion(QuizQuestion question);

    Task<QuizAttempt?> GetAttemptByIdAsync(Guid attemptId, CancellationToken cancellationToken);
    Task<List<QuizAttempt>> GetAttemptsByStudentAsync(Guid quizId, Guid studentUserId, CancellationToken cancellationToken);
    Task<List<QuizAttempt>> GetAttemptsByQuizAsync(Guid quizId, CancellationToken cancellationToken);
    Task<List<QuizAttempt>> GetMyAttemptsAsync(Guid studentUserId, CancellationToken cancellationToken);
    Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken);
    Task AddAnswerAsync(QuizAttemptAnswer answer, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
