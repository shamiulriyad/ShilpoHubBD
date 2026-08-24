using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IAssignmentRepository
{
    Task<Assignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Assignment>> GetByCourseAsync(Guid courseId, CancellationToken cancellationToken);
    Task AddAsync(Assignment assignment, CancellationToken cancellationToken);
    void Remove(Assignment assignment);

    Task<AssignmentSubmission?> GetSubmissionByIdAsync(Guid submissionId, CancellationToken cancellationToken);
    Task<AssignmentSubmission?> GetSubmissionByStudentAsync(Guid assignmentId, Guid studentUserId, CancellationToken cancellationToken);
    Task<List<AssignmentSubmission>> GetSubmissionsByStudentAsync(Guid studentUserId, CancellationToken cancellationToken);
    Task AddSubmissionAsync(AssignmentSubmission submission, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
