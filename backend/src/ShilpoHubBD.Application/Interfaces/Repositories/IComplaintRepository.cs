using ShilpoHubBD.Application.DTOs.Governance;
using ShilpoHubBD.Domain.Entities.Governance;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IComplaintRepository
{
    Task AddAsync(Complaint complaint, CancellationToken cancellationToken);

    void Remove(Complaint complaint);

    Task<Complaint?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<(List<Complaint> Items, int TotalCount)> GetPagedAsync(
        ComplaintQueryParameters query, CancellationToken cancellationToken);

    Task<bool> ReferenceExistsAsync(string referenceCode, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);

    Task<bool> OrderExistsAsync(Guid orderId, CancellationToken cancellationToken);

    Task<bool> FlagExistsAsync(Guid flagId, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
