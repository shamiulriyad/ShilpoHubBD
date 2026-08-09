using ShilpoHubBD.Application.DTOs.ManufacturingPartnership;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IPartnershipRepository
{
    Task<ManufacturingPartnership?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<ManufacturingMilestone?> GetMilestoneAsync(Guid partnershipId, Guid milestoneId, CancellationToken cancellationToken);
    Task<(List<ManufacturingPartnership> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, PartnershipQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ManufacturingPartnership> Items, int TotalCount)> GetPagedForProducerAsync(Guid producerId, PartnershipQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ManufacturingPartnership> Items, int TotalCount)> GetPagedAllAsync(PartnershipQueryParameters parameters, CancellationToken cancellationToken);
    Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);
    Task AddAsync(ManufacturingPartnership partnership, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
