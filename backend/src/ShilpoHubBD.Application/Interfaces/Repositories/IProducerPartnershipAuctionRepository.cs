using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerPartnershipAuctionRepository
{
    Task<ProducerPartnershipAuction?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<ProducerPartnershipAuction> Items, int TotalCount)> GetPagedAsync(
        ProducerPartnershipAuctionQueryParameters parameters, CancellationToken cancellationToken);
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken);
    Task AddAsync(ProducerPartnershipAuction auction, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
