using ShilpoHubBD.Application.DTOs.ProducerPartnership;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>
/// Bid placement and history. Every rule (eligibility, timing, minimum amount) is re-validated here
/// server-side regardless of what the frontend already checked, and concurrent bids on the same lot
/// are resolved by an atomic, guarded database update (see <c>IProducerPartnershipAuctionLotRepository.TryPlaceBidAsync</c>).
/// </summary>
public interface IProducerPartnershipAuctionBidService
{
    Task<ProducerPartnershipAuctionBidDto> PlaceBidAsync(
        Guid auctionId, Guid lotId, Guid businessPartnerId, PlaceProducerPartnershipAuctionBidRequest request, CancellationToken cancellationToken);

    Task<List<ProducerPartnershipAuctionBidDto>> GetMyBidsAsync(
        Guid auctionId, Guid? lotId, Guid businessPartnerId, CancellationToken cancellationToken);

    /// <summary>Admin-only: full bid history for a lot, including every bidder's identity.</summary>
    Task<List<ProducerPartnershipAuctionBidHistoryEntryDto>> GetBidHistoryAsync(Guid auctionId, Guid lotId, CancellationToken cancellationToken);
}
