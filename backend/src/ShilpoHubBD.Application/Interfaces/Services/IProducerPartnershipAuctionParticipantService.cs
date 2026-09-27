using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProducerPartnershipAuctionParticipantService
{
    Task<ProducerPartnershipAuctionParticipantDto> ApplyAsync(Guid auctionId, Guid businessPartnerId, CancellationToken cancellationToken);

    Task<ProducerPartnershipAuctionParticipantDto> DecideAsync(
        Guid auctionId, Guid participantId, Guid decidedByUserId, DecideProducerPartnershipAuctionParticipantRequest request, CancellationToken cancellationToken);

    Task<List<ProducerPartnershipAuctionParticipantDto>> GetForAuctionAsync(Guid auctionId, CancellationToken cancellationToken);
    Task<ProducerPartnershipAuctionParticipantDto?> GetMineAsync(Guid auctionId, Guid businessPartnerId, CancellationToken cancellationToken);
}
