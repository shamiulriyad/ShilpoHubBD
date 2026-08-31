using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly ShilpoHubDbContext _context;

    public QuizRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Quiz> WithDetails()
        => _context.Quizzes
            .Include(q => q.Course).ThenInclude(c => c.Mentor)
            .Include(q => q.Course).ThenInclude(c => c.TrainerProfile)
            .Include(q => q.Questions).ThenInclude(q => q.Options)
            .Include(q => q.Attempts)
            .AsSplitQuery();

    public Task<Quiz?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(q => q.Id == id, cancellationToken);

    public Task<List<Quiz>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken)
        => WithDetails().Where(q => q.CourseId == courseId).OrderByDescending(q => q.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Quiz quiz, CancellationToken cancellationToken)
        => await _context.Quizzes.AddAsync(quiz, cancellationToken);

    public void Remove(Quiz quiz)
        => _context.Quizzes.Remove(quiz);

    public Task<QuizQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken)
        => _context.QuizQuestions
            .Include(q => q.Quiz)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

    public async Task AddQuestionAsync(QuizQuestion question, CancellationToken cancellationToken)
        => await _context.QuizQuestions.AddAsync(question, cancellationToken);

    public void RemoveQuestion(QuizQuestion question)
        => _context.QuizQuestions.Remove(question);

    private IQueryable<QuizAttempt> AttemptsWithDetails()
        => _context.QuizAttempts
            .Include(a => a.Quiz).ThenInclude(q => q.Course).ThenInclude(c => c.Mentor)
            .Include(a => a.Quiz).ThenInclude(q => q.Course).ThenInclude(c => c.TrainerProfile)
            .Include(a => a.Student)
            .Include(a => a.Answers).ThenInclude(ans => ans.QuizQuestion).ThenInclude(q => q.Options)
            .Include(a => a.Answers).ThenInclude(ans => ans.SelectedOption)
            .AsSplitQuery();

    public Task<QuizAttempt?> GetAttemptByIdAsync(Guid attemptId, CancellationToken cancellationToken)
        => AttemptsWithDetails().FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);

    public Task<List<QuizAttempt>> GetAttemptsByStudentAsync(Guid quizId, Guid studentUserId, CancellationToken cancellationToken)
        => AttemptsWithDetails()
            .Where(a => a.QuizId == quizId && a.StudentUserId == studentUserId)
            .OrderByDescending(a => a.AttemptNumber)
            .ToListAsync(cancellationToken);

    public Task<List<QuizAttempt>> GetAttemptsByQuizAsync(Guid quizId, CancellationToken cancellationToken)
        => AttemptsWithDetails()
            .Where(a => a.QuizId == quizId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(cancellationToken);

    public Task<List<QuizAttempt>> GetMyAttemptsAsync(Guid studentUserId, CancellationToken cancellationToken)
        => AttemptsWithDetails()
            .Where(a => a.StudentUserId == studentUserId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAttemptAsync(QuizAttempt attempt, CancellationToken cancellationToken)
        => await _context.QuizAttempts.AddAsync(attempt, cancellationToken);

    public async Task AddAnswerAsync(QuizAttemptAnswer answer, CancellationToken cancellationToken)
        => await _context.QuizAttemptAnswers.AddAsync(answer, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
