using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Repositories;

public class ExamRepository : IExamRepository
{
    private readonly ShilpoHubDbContext _context;

    public ExamRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Exam> WithDetails()
        => _context.Exams
            .Include(e => e.Course).ThenInclude(c => c.Mentor)
            .Include(e => e.Course).ThenInclude(c => c.TrainerProfile)
            .Include(e => e.Questions).ThenInclude(q => q.Options)
            .Include(e => e.Attempts)
            .AsSplitQuery();

    public Task<Exam?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<List<Exam>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken)
        => WithDetails().Where(e => e.CourseId == courseId).OrderByDescending(e => e.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Exam exam, CancellationToken cancellationToken)
        => await _context.Exams.AddAsync(exam, cancellationToken);

    public void Remove(Exam exam)
        => _context.Exams.Remove(exam);

    public Task<ExamQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken)
        => _context.ExamQuestions
            .Include(q => q.Exam)
            .Include(q => q.Options)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

    public async Task AddQuestionAsync(ExamQuestion question, CancellationToken cancellationToken)
        => await _context.ExamQuestions.AddAsync(question, cancellationToken);

    public void RemoveQuestion(ExamQuestion question)
        => _context.ExamQuestions.Remove(question);

    private IQueryable<ExamAttempt> AttemptsWithDetails()
        => _context.ExamAttempts
            .Include(a => a.Exam).ThenInclude(e => e.Course).ThenInclude(c => c.Mentor)
            .Include(a => a.Exam).ThenInclude(e => e.Course).ThenInclude(c => c.TrainerProfile)
            .Include(a => a.Student)
            .Include(a => a.Answers).ThenInclude(ans => ans.ExamQuestion).ThenInclude(q => q.Options)
            .Include(a => a.Answers).ThenInclude(ans => ans.SelectedOption)
            .AsSplitQuery();

    public Task<ExamAttempt?> GetAttemptByIdAsync(Guid attemptId, CancellationToken cancellationToken)
        => AttemptsWithDetails().FirstOrDefaultAsync(a => a.Id == attemptId, cancellationToken);

    public Task<List<ExamAttempt>> GetAttemptsByStudentAsync(Guid examId, Guid studentUserId, CancellationToken cancellationToken)
        => AttemptsWithDetails()
            .Where(a => a.ExamId == examId && a.StudentUserId == studentUserId)
            .OrderByDescending(a => a.AttemptNumber)
            .ToListAsync(cancellationToken);

    public Task<List<ExamAttempt>> GetAttemptsByExamAsync(Guid examId, CancellationToken cancellationToken)
        => AttemptsWithDetails()
            .Where(a => a.ExamId == examId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(cancellationToken);

    public Task<List<ExamAttempt>> GetMyAttemptsAsync(Guid studentUserId, CancellationToken cancellationToken)
        => AttemptsWithDetails()
            .Where(a => a.StudentUserId == studentUserId)
            .OrderByDescending(a => a.StartedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAttemptAsync(ExamAttempt attempt, CancellationToken cancellationToken)
        => await _context.ExamAttempts.AddAsync(attempt, cancellationToken);

    public async Task AddAnswerAsync(ExamAttemptAnswer answer, CancellationToken cancellationToken)
        => await _context.ExamAttemptAnswers.AddAsync(answer, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
