using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerPartnershipAuctionLotService
{
    Task<ProducerPartnershipAuctionLotDetailDto> AddLotAsync(Guid auctionId, Guid producerId, Guid currentUserId, CancellationToken cancellationToken);
    Task RemoveLotAsync(Guid auctionId, Guid lotId, CancellationToken cancellationToken);

    Task<List<ProducerPartnershipAuctionLotListItemDto>> GetLotsForAuctionAsync(
        Guid auctionId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);

    Task<ProducerPartnershipAuctionLotDetailDto> GetLotDetailAsync(
        Guid auctionId, Guid lotId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
}
