using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IHeritageInnovationSubmissionService
{
    Task<PagedResult<HeritageInnovationSubmissionListItemDto>> GetAccessibleAsync(
        Guid userId, bool isReviewer, HeritageInnovationSubmissionQueryParameters query, CancellationToken cancellationToken);

    Task<HeritageInnovationSubmissionDetailDto> GetByIdAsync(
        Guid userId, bool isReviewer, Guid submissionId, CancellationToken cancellationToken);

    Task<HeritageInnovationSubmissionDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreateHeritageInnovationSubmissionRequest request, CancellationToken cancellationToken);

    Task<HeritageInnovationSubmissionDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid submissionId, UpdateHeritageInnovationSubmissionRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken);

    Task<HeritageInnovationSubmissionDetailDto> SubmitAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken);
    Task<HeritageInnovationSubmissionDetailDto> WithdrawAsync(Guid userId, Guid submissionId, CancellationToken cancellationToken);

    Task<SubmissionTeamMemberDto> AddTeamMemberAsync(
        Guid userId, Guid submissionId, AddSubmissionTeamMemberRequest request, CancellationToken cancellationToken);
    Task RemoveTeamMemberAsync(Guid userId, Guid submissionId, Guid memberId, CancellationToken cancellationToken);

    Task<SubmissionReviewDto> AddReviewAsync(
        Guid userId, bool isReviewer, Guid submissionId, CreateSubmissionReviewRequest request, CancellationToken cancellationToken);

    Task<List<SubmissionEventDto>> GetHistoryAsync(
        Guid userId, bool isReviewer, Guid submissionId, int take, CancellationToken cancellationToken);
}
