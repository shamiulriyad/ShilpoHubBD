using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Services.ProducerPartnership;

public class ProducerPartnershipAuctionLotService : IProducerPartnershipAuctionLotService
{
    private readonly IProducerPartnershipAuctionRepository _auctionRepository;
    private readonly IProducerPartnershipAuctionLotRepository _lotRepository;
    private readonly IProducerPartnershipAuctionParticipantRepository _participantRepository;
    private readonly IProducerPartnershipAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;
    private readonly ISupplierDiscoveryService _supplierDiscoveryService;

    public ProducerPartnershipAuctionLotService(
        IProducerPartnershipAuctionRepository auctionRepository, IProducerPartnershipAuctionLotRepository lotRepository,
        IProducerPartnershipAuctionParticipantRepository participantRepository, IProducerPartnershipAgreementRepository agreementRepository,
        IUserRepository userRepository, ISupplierDiscoveryService supplierDiscoveryService)
    {
        _auctionRepository = auctionRepository;
        _lotRepository = lotRepository;
        _participantRepository = participantRepository;
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
        _supplierDiscoveryService = supplierDiscoveryService;
    }

    public async Task<ProducerPartnershipAuctionLotDetailDto> AddLotAsync(
        Guid auctionId, Guid producerId, Guid currentUserId, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(auctionId, cancellationToken)
            ?? throw new NotFoundException("Producer partnership auction not found.");

        if (auction.Status is not (ProducerPartnershipAuctionStatus.Draft or ProducerPartnershipAuctionStatus.Scheduled or ProducerPartnershipAuctionStatus.RegistrationOpen))
        {
            throw new ConflictException("Producers can only be added while the auction is in Draft, Scheduled or RegistrationOpen.");
        }

        var producer = await _userRepository.GetByIdWithRolesAsync(producerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        if (await _lotRepository.ExistsForProducerAsync(auctionId, producerId, cancellationToken))
        {
            throw new ConflictException("This producer is already entered in this auction.");
        }

        if (await _agreementRepository.HasActiveAgreementAsync(producerId, null, cancellationToken))
        {
            throw new ConflictException("This producer already has an active partnership and cannot be entered into a new auction.");
        }

        var now = DateTime.UtcNow;
        var lot = new ProducerPartnershipAuctionLot
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            ProducerId = producerId,
            StartingBid = auction.MinimumStartingBid,
            Status = ProducerPartnershipAuctionLotStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _lotRepository.AddAsync(lot, cancellationToken);
        await _lotRepository.SaveChangesAsync(cancellationToken);

        return await GetLotDetailAsync(auctionId, lot.Id, currentUserId, isAdmin: true, cancellationToken);
    }

    public async Task RemoveLotAsync(Guid auctionId, Guid lotId, CancellationToken cancellationToken)
    {
        var lot = await _lotRepository.GetByIdAsync(lotId, cancellationToken);
        if (lot is null || lot.AuctionId != auctionId)
        {
            throw new NotFoundException("Auction lot not found.");
        }

        if (lot.Status != ProducerPartnershipAuctionLotStatus.Pending)
        {
            throw new ConflictException("Only a lot that has not gone live yet can be removed.");
        }

        lot.Status = ProducerPartnershipAuctionLotStatus.Withdrawn;
        lot.UpdatedAt = DateTime.UtcNow;
        await _lotRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<ProducerPartnershipAuctionLotListItemDto>> GetLotsForAuctionAsync(
        Guid auctionId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureViewerEligibleAsync(auctionId, currentUserId, isAdmin, cancellationToken);

        var lots = await _lotRepository.GetForAuctionAsync(auctionId, cancellationToken);
        return lots
            .Where(l => l.Status != ProducerPartnershipAuctionLotStatus.Withdrawn)
            .Select(l => ToListItemDto(l, currentUserId, isAdmin))
            .ToList();
    }

    public async Task<ProducerPartnershipAuctionLotDetailDto> GetLotDetailAsync(
        Guid auctionId, Guid lotId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        await EnsureViewerEligibleAsync(auctionId, currentUserId, isAdmin, cancellationToken);

        var lot = await _lotRepository.GetByIdWithBidsAsync(lotId, cancellationToken);
        if (lot is null || lot.AuctionId != auctionId)
        {
            throw new NotFoundException("Auction lot not found.");
        }

        var profile = await _supplierDiscoveryService.GetBusinessProfileAsync(lot.ProducerId, cancellationToken);
        var (minimumNextBid, isCurrentUserHighest, highestBidderName, winnerId, winnerName, winningAmount) =
            ComputeDerivedFields(lot, currentUserId, isAdmin);

        var currentUserHighestBid = lot.Bids
            .Where(b => b.BusinessPartnerId == currentUserId)
            .Select(b => (decimal?)b.Amount)
            .DefaultIfEmpty(null)
            .Max();

        return new ProducerPartnershipAuctionLotDetailDto
        {
            Id = lot.Id,
            AuctionId = lot.AuctionId,
            ProducerId = lot.ProducerId,
            ProducerProfile = profile,
            StartingBid = lot.StartingBid,
            CurrentHighestBid = lot.CurrentHighestBid,
            BidCount = lot.BidCount,
            Status = lot.Status,
            MinimumNextBid = minimumNextBid,
            IsCurrentUserHighestBidder = isCurrentUserHighest,
            CurrentUserHighestBid = currentUserHighestBid,
            CurrentHighestBidderName = highestBidderName,
            WinnerId = winnerId,
            WinnerName = winnerName,
            WinningAmount = winningAmount,
            CreatedAt = lot.CreatedAt,
            UpdatedAt = lot.UpdatedAt,
        };
    }

    private async Task EnsureViewerEligibleAsync(Guid auctionId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (isAdmin)
        {
            return;
        }

        var participant = await _participantRepository.GetForBusinessPartnerAsync(auctionId, currentUserId, cancellationToken);
        if (participant is null || participant.Status != ProducerPartnershipAuctionParticipantStatus.Approved)
        {
            throw new UnauthorizedAccessException("You must be an approved participant in this auction to view its lots.");
        }
    }

    private static ProducerPartnershipAuctionLotListItemDto ToListItemDto(
        ProducerPartnershipAuctionLot lot, Guid currentUserId, bool isAdmin)
    {
        var (minimumNextBid, isCurrentUserHighest, highestBidderName, winnerId, winnerName, winningAmount) =
            ComputeDerivedFields(lot, currentUserId, isAdmin);

        return new ProducerPartnershipAuctionLotListItemDto
        {
            Id = lot.Id,
            AuctionId = lot.AuctionId,
            ProducerId = lot.ProducerId,
            ProducerName = lot.Producer.FullName,
            StartingBid = lot.StartingBid,
            CurrentHighestBid = lot.CurrentHighestBid,
            BidCount = lot.BidCount,
            Status = lot.Status,
            MinimumNextBid = minimumNextBid,
            IsCurrentUserHighestBidder = isCurrentUserHighest,
            CurrentHighestBidderName = highestBidderName,
            WinnerId = winnerId,
            WinnerName = winnerName,
            WinningAmount = winningAmount,
        };
    }

    private static (decimal MinimumNextBid, bool IsCurrentUserHighest, string? HighestBidderName, Guid? WinnerId, string? WinnerName, decimal? WinningAmount)
        ComputeDerivedFields(ProducerPartnershipAuctionLot lot, Guid currentUserId, bool isAdmin)
    {
        var minimumNextBid = lot.CurrentHighestBid.HasValue
            ? lot.CurrentHighestBid.Value + lot.Auction.MinimumBidIncrement
            : lot.StartingBid;

        var isCurrentUserHighest = lot.CurrentHighestBidderId == currentUserId;

        // Only reveal the current highest bidder's identity to an admin, or to that bidder themself.
        var highestBidderName = isAdmin ? lot.CurrentHighestBidder?.FullName
            : isCurrentUserHighest ? lot.CurrentHighestBidder?.FullName
            : null;

        Guid? winnerId = null;
        string? winnerName = null;
        decimal? winningAmount = null;
        if (lot.Status == ProducerPartnershipAuctionLotStatus.Awarded && lot.WinningBid is not null)
        {
            winnerId = lot.WinningBid.BusinessPartnerId;
            winningAmount = lot.WinningBid.Amount;
            // The winner's own identity is always visible to them and to admin; other bidders just see "Awarded".
            if (isAdmin || winnerId == currentUserId)
            {
                winnerName = lot.WinningBid.BusinessPartner.FullName;
            }
        }

        return (minimumNextBid, isCurrentUserHighest, highestBidderName, winnerId, winnerName, winningAmount);
    }
}
