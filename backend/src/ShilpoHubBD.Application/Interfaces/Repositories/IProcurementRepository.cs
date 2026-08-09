using ShilpoHubBD.Application.DTOs.Procurement;
using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProcurementRepository
{
    Task<ProcurementRequest?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<ProcurementRequest> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(Guid businessPartnerId, ProcurementQueryParameters parameters, CancellationToken cancellationToken);
    Task<(List<ProcurementRequest> Items, int TotalCount)> GetPagedAllAsync(ProcurementQueryParameters parameters, CancellationToken cancellationToken);
    Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken);
    Task AddAsync(ProcurementRequest procurementRequest, CancellationToken cancellationToken);
    Task AddStatusEventAsync(ProcurementStatusEvent statusEvent, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
