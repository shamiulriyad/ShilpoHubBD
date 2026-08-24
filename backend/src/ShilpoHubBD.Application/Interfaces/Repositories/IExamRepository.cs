using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IExamRepository
{
    Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Exam>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task AddAsync(Exam exam, CancellationToken cancellationToken);
    void Remove(Exam exam);

    Task<ExamQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken);
    Task AddQuestionAsync(ExamQuestion question, CancellationToken cancellationToken);
    void RemoveQuestion(ExamQuestion question);

    Task<ExamAttempt?> GetAttemptByIdAsync(Guid attemptId, CancellationToken cancellationToken);
    Task<List<ExamAttempt>> GetAttemptsByStudentAsync(Guid examId, Guid studentUserId, CancellationToken cancellationToken);
    Task<List<ExamAttempt>> GetAttemptsByExamAsync(Guid examId, CancellationToken cancellationToken);
    Task<List<ExamAttempt>> GetMyAttemptsAsync(Guid studentUserId, CancellationToken cancellationToken);
    Task AddAttemptAsync(ExamAttempt attempt, CancellationToken cancellationToken);
    Task AddAnswerAsync(ExamAttemptAnswer answer, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
