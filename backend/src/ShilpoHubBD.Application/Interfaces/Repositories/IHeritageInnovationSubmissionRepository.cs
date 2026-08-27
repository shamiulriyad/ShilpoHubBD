using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IHeritageInnovationSubmissionRepository
{
    Task<HeritageInnovationSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<HeritageInnovationSubmission?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<HeritageInnovationSubmission> Items, int TotalCount)> GetPagedAsync(
        Guid userId, bool isReviewer, HeritageInnovationSubmissionQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(HeritageInnovationSubmission submission, CancellationToken cancellationToken);
    void Remove(HeritageInnovationSubmission submission);

    Task<SubmissionTeamMember?> GetTeamMemberByIdAsync(Guid memberId, CancellationToken cancellationToken);
    Task AddTeamMemberAsync(SubmissionTeamMember member, CancellationToken cancellationToken);
    void RemoveTeamMember(SubmissionTeamMember member);

    Task AddReviewAsync(SubmissionReview review, CancellationToken cancellationToken);
    Task<List<SubmissionReview>> GetReviewsAsync(Guid submissionId, CancellationToken cancellationToken);

    Task AddEventAsync(SubmissionEvent submissionEvent, CancellationToken cancellationToken);
    Task<List<SubmissionEvent>> GetHistoryAsync(Guid submissionId, int take, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
