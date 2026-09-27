using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProducerPartnershipAuctionParticipantRepository
{
    Task<ProducerPartnershipAuctionParticipant?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionParticipant?> GetForBusinessPartnerAsync(Guid auctionId, Guid businessPartnerId, CancellationToken cancellationToken);
    Task<List<ProducerPartnershipAuctionParticipant>> GetForAuctionAsync(Guid auctionId, CancellationToken cancellationToken);
    Task AddAsync(ProducerPartnershipAuctionParticipant participant, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
