using ShilpoHubBD.Application.DTOs.Assessment;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IAssignmentService
{
    Task<AssignmentDto> CreateAsync(Guid userId, Guid courseId, CreateAssignmentRequest request, CancellationToken cancellationToken);

    Task<AssignmentDto> UpdateAsync(Guid userId, Guid assignmentId, UpdateAssignmentRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken);

    Task<AssignmentDto> GetByIdAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken);

    Task<List<AssignmentListItemDto>> GetByCourseAsync(Guid userId, Guid courseId, CancellationToken cancellationToken);

    Task<AssignmentSubmissionDto> SubmitAsync(Guid studentUserId, Guid assignmentId, SubmitAssignmentRequest request, CancellationToken cancellationToken);

    Task<AssignmentSubmissionDto> GetMySubmissionAsync(Guid studentUserId, Guid assignmentId, CancellationToken cancellationToken);

    Task<List<AssignmentSubmissionDto>> GetSubmissionsAsync(Guid userId, Guid assignmentId, CancellationToken cancellationToken);

    Task<AssignmentSubmissionDto> GradeAsync(Guid userId, Guid submissionId, GradeAssignmentSubmissionRequest request, CancellationToken cancellationToken);
}
