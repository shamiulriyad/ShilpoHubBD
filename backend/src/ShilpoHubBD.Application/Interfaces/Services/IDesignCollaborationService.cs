using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IDesignCollaborationService
{
    Task<ProjectDto> CreateAsync(Guid businessPartnerId, CreateProjectRequest request, CancellationToken cancellationToken);

    Task<PagedResult<ProjectListItemDto>> GetForBusinessPartnerAsync(Guid businessPartnerId, bool isAdmin, ProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<PagedResult<ProjectListItemDto>> GetForProducerAsync(Guid producerId, ProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<ProjectDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<ProjectDto> RespondAsync(Guid id, Guid producerId, CollaborationResponseRequest request, CancellationToken cancellationToken);
    Task<DesignCommentDto> AddCommentAsync(Guid id, Guid currentUserId, bool isAdmin, AddCommentRequest request, CancellationToken cancellationToken);
    Task<DesignFileDto> AddFileAsync(Guid id, Guid currentUserId, bool isAdmin, DesignFileInput request, CancellationToken cancellationToken);
    Task<DesignRevisionDto> SubmitRevisionAsync(Guid id, Guid producerId, SubmitRevisionRequest request, CancellationToken cancellationToken);
    Task<DesignRevisionDto> DecideRevisionAsync(Guid id, Guid revisionId, Guid businessPartnerId, bool isAdmin, RevisionDecisionRequest request, CancellationToken cancellationToken);
    Task<ProjectDto> CompleteAsync(Guid id, Guid businessPartnerId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProjectDto> CancelAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
