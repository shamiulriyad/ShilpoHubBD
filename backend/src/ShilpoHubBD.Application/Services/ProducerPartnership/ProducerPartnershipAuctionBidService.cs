using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Services.ProducerPartnership;

public class ProducerPartnershipAuctionBidService : IProducerPartnershipAuctionBidService
{
    private readonly IProducerPartnershipAuctionRepository _auctionRepository;
    private readonly IProducerPartnershipAuctionLotRepository _lotRepository;
    private readonly IProducerPartnershipAuctionParticipantRepository _participantRepository;
    private readonly IProducerPartnershipAgreementRepository _agreementRepository;

    public ProducerPartnershipAuctionBidService(
        IProducerPartnershipAuctionRepository auctionRepository, IProducerPartnershipAuctionLotRepository lotRepository,
        IProducerPartnershipAuctionParticipantRepository participantRepository, IProducerPartnershipAgreementRepository agreementRepository)
    {
        _auctionRepository = auctionRepository;
        _lotRepository = lotRepository;
        _participantRepository = participantRepository;
        _agreementRepository = agreementRepository;
    }

    public async Task<ProducerPartnershipAuctionBidDto> PlaceBidAsync(
        Guid auctionId, Guid lotId, Guid businessPartnerId, PlaceProducerPartnershipAuctionBidRequest request, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
            ?? throw new NotFoundException("Producer partnership auction not found.");

        if (auction.Status != ProducerPartnershipAuctionStatus.Live)
        {
            throw new ConflictException("Bidding is not open for this auction.");
        }

        var now = DateTime.UtcNow;
        if (auction.BiddingClosesAt.HasValue && now > auction.BiddingClosesAt.Value)
        {
            throw new ConflictException("Bidding has closed for this auction.");
        }

        var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
        if (lot is null || lot.AuctionId != auctionId)
        {
            throw new NotFoundException("Auction lot not found.");
        }

        if (lot.Status != ProducerPartnershipAuctionLotStatus.Open)
        {
            throw new ConflictException("This lot is not open for bidding.");
        }

        var participant = await _participantRepository.GetForBusinessPartnerAsync(auctionId, businessPartnerId, cancellationToken);
        if (participant is null || participant.Status != ProducerPartnershipAuctionParticipantStatus.Approved)
        {
            throw new UnauthorizedAccessException("You must be an approved participant in this auction to bid.");
        }

        if (await _agreementRepository.HasActiveAgreementAsync(lot.ProducerId, businessPartnerId, cancellationToken))
        {
            throw new ConflictException("You already have an active partnership with this producer.");
        }

        var minimumAcceptable = lot.CurrentHighestBid.HasValue
            ? lot.CurrentHighestBid.Value + auction.MinimumBidIncrement
            : lot.StartingBid;

        if (request.Amount < minimumAcceptable)
        {
            throw new ConflictException($"Your bid must be at least {minimumAcceptable}.");
        }

        var bid = new ProducerPartnershipAuctionBid
        {
            Id = Guid.NewGuid(),
            LotId = lotId,
            BusinessPartnerId = businessPartnerId,
            Amount = request.Amount,
            PlacedAt = now,
        };

        // The guard inside TryPlaceBidAsync re-checks "lot Open" and "amount beats current highest by
        // the increment" atomically in the database — if another bid won that race between our reads
        // above and now, this returns false and nothing is written.
        var placed = await _lotRepository.TryPlaceBidAsync(bid, auction.MinimumBidIncrement, cancellationToken);
        if (!placed)
        {
            throw new ConflictException("Someone just placed a higher bid on this lot. Refresh and try again.");
        }

        return new ProducerPartnershipAuctionBidDto
        {
            Id = bid.Id,
            LotId = lotId,
            ProducerId = lot.ProducerId,
            ProducerName = lot.Producer.FullName,
            Amount = bid.Amount,
            PlacedAt = bid.PlacedAt,
            IsCurrentHighest = true,
        };
    }

    public async Task<List<ProducerPartnershipAuctionBidDto>> GetMyBidsAsync(
        Guid auctionId, Guid? lotId, Guid businessPartnerId, CancellationToken cancellationToken)
    {
        var bids = await _lotRepository.GetBidsForBusinessPartnerAsync(businessPartnerId, lotId, auctionId, cancellationToken);

        return bids.Select(b => new ProducerPartnershipAuctionBidDto
        {
            Id = b.Id,
            LotId = b.LotId,
            ProducerId = b.Lot.ProducerId,
            ProducerName = b.Lot.Producer.FullName,
            Amount = b.Amount,
            PlacedAt = b.PlacedAt,
            IsCurrentHighest = b.Lot.CurrentHighestBidderId == businessPartnerId && b.Lot.CurrentHighestBid == b.Amount,
        }).ToList();
    }

    public async Task<List<ProducerPartnershipAuctionBidHistoryEntryDto>> GetBidHistoryAsync(
        Guid auctionId, Guid lotId, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
        if (lot is null || lot.AuctionId != auctionId)
        {
            throw new NotFoundException("Auction lot not found.");
        }

        var bids = await _lotRepository.GetBidsForLotAsync(lotId, cancellationToken);
        return bids.Select(b => new ProducerPartnershipAuctionBidHistoryEntryDto
        {
            Id = b.Id,
            BusinessPartnerId = b.BusinessPartnerId,
            BusinessPartnerName = b.BusinessPartner.FullName,
            Amount = b.Amount,
            PlacedAt = b.PlacedAt,
        }).ToList();
    }
}
