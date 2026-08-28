using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Data.Repositories;

public class AssignmentRepository : IAssignmentRepository
{
    private readonly ShilpoHubDbContext _context;

    public AssignmentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Assignment> WithDetails()
        => _context.Assignments
            .Include(a => a.Course).ThenInclude(c => c.Mentor)
            .Include(a => a.Course).ThenInclude(c => c.TrainerProfile)
            .Include(a => a.Submissions).ThenInclude(s => s.Student)
            .AsSplitQuery();

    public Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public Task<List<Assignment>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken)
        => WithDetails().Where(a => a.CourseId == courseId).OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task AddAsync(Assignment assignment, CancellationToken cancellationToken)
        => await _context.Assignments.AddAsync(assignment, cancellationToken);

    public void Remove(Assignment assignment)
        => _context.Assignments.Remove(assignment);

    public Task<AssignmentSubmission?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken cancellationToken)
        => _context.AssignmentSubmissions
            .Include(s => s.Assignment).ThenInclude(a => a.Course).ThenInclude(c => c.Mentor)
            .Include(s => s.Assignment).ThenInclude(a => a.Course).ThenInclude(c => c.TrainerProfile)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.Id == submissionId, cancellationToken);

    public Task<AssignmentSubmission?> GetSubmissionByStudentAsync(Guid assignmentId, Guid studentUserId, CancellationToken cancellationToken)
        => _context.AssignmentSubmissions
            .Include(s => s.Assignment)
            .Include(s => s.Student)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.StudentUserId == studentUserId, cancellationToken);

    public Task<List<AssignmentSubmission>> GetSubmissionsByStudentAsync(Guid studentUserId, CancellationToken cancellationToken)
        => _context.AssignmentSubmissions
            .Include(s => s.Assignment)
            .Where(s => s.StudentUserId == studentUserId)
            .OrderByDescending(s => s.SubmittedAt)
            .ToListAsync(cancellationToken);

    public async Task AddSubmissionAsync(AssignmentSubmission submission, CancellationToken cancellationToken)
        => await _context.AssignmentSubmissions.AddAsync(submission, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
