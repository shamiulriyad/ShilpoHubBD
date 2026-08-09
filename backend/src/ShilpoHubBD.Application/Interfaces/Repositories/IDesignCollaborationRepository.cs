using ShilpoHubBD.Application.DTOs.DesignCollaboration;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IDesignCollaborationRepository
{
    Task<DesignCollaborationProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<DesignCollaborationProject> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, ProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<DesignCollaborationProject> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, ProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<DesignCollaborationProject> Items, int TotalCount)> GetPagedAllAsync(ProjectQueryParameters parameters, CancellationToken cancellationToken);
    Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);
    Task AddAsync(DesignCollaborationProject project, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
