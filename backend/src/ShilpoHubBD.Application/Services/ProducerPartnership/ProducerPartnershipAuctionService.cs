using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Services.ProducerPartnership;

public class ProducerPartnershipAuctionService : IProducerPartnershipAuctionService
{
    private readonly IProducerPartnershipAuctionRepository _auctionRepository;
    private readonly IProducerPartnershipAuctionLotRepository _lotRepository;
    private readonly IUserRepository _userRepository;
    private readonly IProducerPartnershipAgreementService _agreementService;

    public ProducerPartnershipAuctionService(
        IProducerPartnershipAuctionRepository auctionRepository, IProducerPartnershipAuctionLotRepository lotRepository,
        IUserRepository userRepository, IProducerPartnershipAgreementService agreementService)
    {
        _auctionRepository = auctionRepository;
        _lotRepository = lotRepository;
        _userRepository = userRepository;
        _agreementService = agreementService;
    }

    public async Task<ProducerPartnershipAuctionDto> CreateAsync(
        Guid managedByUserId, CreateProducerPartnershipAuctionRequest request, CancellationToken cancellationToken)
    {
        var manager = await _userRepository.GetByIdAsync(managedByUserId, cancellationToken)
            ?? throw new NotFoundException("Managing admin not found.");

        var now = DateTime.UtcNow;
        var biddingClosesAt = request.BiddingClosesAt;
        if (!biddingClosesAt.HasValue && request.BiddingOpensAt.HasValue && request.AuctionDurationHours.HasValue)
        {
            biddingClosesAt = request.BiddingOpensAt.Value.AddHours(request.AuctionDurationHours.Value);
        }

        var auction = new Domain.Entities.ProducerPartnership.ProducerPartnershipAuction
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = await GenerateUniqueSlugAsync(request.Name, cancellationToken),
            AuctionYear = request.AuctionYear,
            Status = ProducerPartnershipAuctionStatus.Draft,
            Description = request.Description.Trim(),
            EligibilityCriteria = string.IsNullOrWhiteSpace(request.EligibilityCriteria) ? null : request.EligibilityCriteria.Trim(),
            BusinessPartnerEligibilityCriteria = string.IsNullOrWhiteSpace(request.BusinessPartnerEligibilityCriteria) ? null : request.BusinessPartnerEligibilityCriteria.Trim(),
            RegistrationOpensAt = request.RegistrationOpensAt,
            RegistrationClosesAt = request.RegistrationClosesAt,
            BiddingOpensAt = request.BiddingOpensAt,
            BiddingClosesAt = biddingClosesAt,
            AuctionDurationHours = request.AuctionDurationHours,
            ParticipationFee = request.ParticipationFee,
            Currency = string.IsNullOrWhiteSpace(request.Currency) ? "BDT" : request.Currency.Trim(),
            MinimumStartingBid = request.MinimumStartingBid,
            MinimumBidIncrement = request.MinimumBidIncrement,
            MaxProducersPerBusinessPartner = request.MaxProducersPerBusinessPartner,
            DefaultPartnershipDurationMonths = request.DefaultPartnershipDurationMonths,
            DefaultRevenueSharePercentage = request.DefaultRevenueSharePercentage,
            SettlementRulesDescription = string.IsNullOrWhiteSpace(request.SettlementRulesDescription) ? null : request.SettlementRulesDescription.Trim(),
            ManagedByUserId = managedByUserId,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _auctionRepository.AddAsync(auction, cancellationToken);
        await _auctionRepository.SaveChangesAsync(cancellationToken);

        auction.ManagedBy = manager;
        return ToDto(auction);
    }

    public async Task<ProducerPartnershipAuctionDto> UpdateAsync(
        Guid id, UpdateProducerPartnershipAuctionRequest request, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Producer partnership auction not found.");

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            auction.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            auction.Status = ParseStatus(request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.Description))
        {
            auction.Description = request.Description.Trim();
        }

        if (request.EligibilityCriteria is not null)
        {
            auction.EligibilityCriteria = string.IsNullOrWhiteSpace(request.EligibilityCriteria) ? null : request.EligibilityCriteria.Trim();
        }

        if (request.BusinessPartnerEligibilityCriteria is not null)
        {
            auction.BusinessPartnerEligibilityCriteria = string.IsNullOrWhiteSpace(request.BusinessPartnerEligibilityCriteria) ? null : request.BusinessPartnerEligibilityCriteria.Trim();
        }

        if (request.RegistrationOpensAt.HasValue) auction.RegistrationOpensAt = request.RegistrationOpensAt;
        if (request.RegistrationClosesAt.HasValue) auction.RegistrationClosesAt = request.RegistrationClosesAt;
        if (request.BiddingOpensAt.HasValue) auction.BiddingOpensAt = request.BiddingOpensAt;
        if (request.BiddingClosesAt.HasValue) auction.BiddingClosesAt = request.BiddingClosesAt;
        if (request.ParticipationFee.HasValue) auction.ParticipationFee = request.ParticipationFee;
        if (request.MinimumStartingBid.HasValue) auction.MinimumStartingBid = request.MinimumStartingBid.Value;
        if (request.MinimumBidIncrement.HasValue) auction.MinimumBidIncrement = request.MinimumBidIncrement.Value;
        if (request.MaxProducersPerBusinessPartner.HasValue) auction.MaxProducersPerBusinessPartner = request.MaxProducersPerBusinessPartner;
        if (request.DefaultPartnershipDurationMonths.HasValue) auction.DefaultPartnershipDurationMonths = request.DefaultPartnershipDurationMonths.Value;
        if (request.DefaultRevenueSharePercentage.HasValue) auction.DefaultRevenueSharePercentage = request.DefaultRevenueSharePercentage;

        if (request.SettlementRulesDescription is not null)
        {
            auction.SettlementRulesDescription = string.IsNullOrWhiteSpace(request.SettlementRulesDescription) ? null : request.SettlementRulesDescription.Trim();
        }

        auction.UpdatedAt = DateTime.UtcNow;

        await _auctionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(auction);
    }

    public async Task<ProducerPartnershipAuctionDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await _auctionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Producer partnership auction not found.");
        return ToDto(auction);
    }

    public async Task<PagedResult<ProducerPartnershipAuctionListItemDto>> GetPagedAsync(
        ProducerPartnershipAuctionQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _auctionRepository.GetPagedAsync(parameters, cancellationToken);

        return new PagedResult<ProducerPartnershipAuctionListItemDto>
        {
            Items = items.Select(a => new ProducerPartnershipAuctionListItemDto
            {
                Id = a.Id,
                Name = a.Name,
                Slug = a.Slug,
                AuctionYear = a.AuctionYear,
                Status = a.Status,
                BiddingOpensAt = a.BiddingOpensAt,
                BiddingClosesAt = a.BiddingClosesAt,
                AgreementCount = a.Agreements.Count,
                LotCount = a.Lots.Count,
                CreatedAt = a.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private async Task<string> GenerateUniqueSlugAsync(string name, CancellationToken cancellationToken)
    {
        var baseSlug = string.Join('-', name.Trim().ToLowerInvariant().Split(
            new[] { ' ', '/', '\\' }, StringSplitOptions.RemoveEmptyEntries));
        if (string.IsNullOrWhiteSpace(baseSlug))
        {
            baseSlug = "producer-partnership-auction";
        }

        var slug = baseSlug;
        var suffix = 1;
        while (await _auctionRepository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{++suffix}";
        }

        return slug;
    }

    private static ProducerPartnershipAuctionStatus ParseStatus(string value)
        => Enum.TryParse<ProducerPartnershipAuctionStatus>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException($"'{value}' is not a valid auction status.");

    private static ProducerPartnershipAuctionDto ToDto(Domain.Entities.ProducerPartnership.ProducerPartnershipAuction auction) => new()
    {
        Id = auction.Id,
        Name = auction.Name,
        Slug = auction.Slug,
        AuctionYear = auction.AuctionYear,
        Status = auction.Status,
        Description = auction.Description,
        EligibilityCriteria = auction.EligibilityCriteria,
        BusinessPartnerEligibilityCriteria = auction.BusinessPartnerEligibilityCriteria,
        RegistrationOpensAt = auction.RegistrationOpensAt,
        RegistrationClosesAt = auction.RegistrationClosesAt,
        BiddingOpensAt = auction.BiddingOpensAt,
        BiddingClosesAt = auction.BiddingClosesAt,
        AuctionDurationHours = auction.AuctionDurationHours,
        ParticipationFee = auction.ParticipationFee,
        Currency = auction.Currency,
        MinimumStartingBid = auction.MinimumStartingBid,
        MinimumBidIncrement = auction.MinimumBidIncrement,
        MaxProducersPerBusinessPartner = auction.MaxProducersPerBusinessPartner,
        DefaultPartnershipDurationMonths = auction.DefaultPartnershipDurationMonths,
        DefaultRevenueSharePercentage = auction.DefaultRevenueSharePercentage,
        SettlementRulesDescription = auction.SettlementRulesDescription,
        ManagedByUserId = auction.ManagedByUserId,
        ManagedByName = auction.ManagedBy?.FullName ?? string.Empty,
        AgreementCount = auction.Agreements.Count,
        LotCount = auction.Lots.Count,
        ParticipantCount = auction.Participants.Count,
        CreatedAt = auction.CreatedAt,
        UpdatedAt = auction.UpdatedAt,
    };

    public async Task<ProducerPartnershipAuctionDto> ScheduleAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await RequireAuctionAsync(id, cancellationToken);
        RequireStatus(auction, ProducerPartnershipAuctionStatus.Draft, "scheduled");

        if (!auction.BiddingOpensAt.HasValue || !auction.BiddingClosesAt.HasValue)
        {
            throw new ConflictException("Set the auction start and end date/time before scheduling it.");
        }

        auction.Status = ProducerPartnershipAuctionStatus.Scheduled;
        auction.UpdatedAt = DateTime.UtcNow;
        await _auctionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(auction);
    }

    public async Task<ProducerPartnershipAuctionDto> OpenRegistrationAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await RequireAuctionAsync(id, cancellationToken);
        RequireStatus(auction, ProducerPartnershipAuctionStatus.Scheduled, "opened for registration");

        auction.Status = ProducerPartnershipAuctionStatus.RegistrationOpen;
        auction.UpdatedAt = DateTime.UtcNow;
        await _auctionRepository.SaveChangesAsync(cancellationToken);
        return ToDto(auction);
    }

    public async Task<ProducerPartnershipAuctionDto> GoLiveAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await RequireAuctionAsync(id, cancellationToken);
        RequireStatus(auction, ProducerPartnershipAuctionStatus.RegistrationOpen, "started");

        var lots = await _lotRepository.GetForAuctionAsync(id, cancellationToken);
        if (lots.Count == 0)
        {
            throw new ConflictException("Add at least one producer lot before starting the auction.");
        }

        var now = DateTime.UtcNow;
        foreach (var lot in lots.Where(l => l.Status == ProducerPartnershipAuctionLotStatus.Pending))
        {
            lot.Status = ProducerPartnershipAuctionLotStatus.Open;
            lot.UpdatedAt = now;
        }

        auction.Status = ProducerPartnershipAuctionStatus.Live;
        auction.BiddingOpensAt ??= now;
        auction.UpdatedAt = now;
        await _lotRepository.SaveChangesAsync(cancellationToken);
        return ToDto(auction);
    }

    public async Task<ProducerPartnershipAuctionDto> EndAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await RequireAuctionAsync(id, cancellationToken);
        RequireStatus(auction, ProducerPartnershipAuctionStatus.Live, "ended");

        var now = DateTime.UtcNow;
        var lots = await _lotRepository.GetForAuctionWithBidsAsync(id, cancellationToken);
        var cap = auction.MaxProducersPerBusinessPartner;
        var winCounts = new Dictionary<Guid, int>();
        var awarded = new List<(Guid LotId, Guid ProducerId, Guid BusinessPartnerId, decimal Amount)>();

        // Process the highest-value lots first so a capped Business Partner keeps their best wins;
        // a lot they'd otherwise have won falls through to the next-highest remaining bidder on it.
        foreach (var lot in lots
            .Where(l => l.Status is ProducerPartnershipAuctionLotStatus.Open or ProducerPartnershipAuctionLotStatus.Pending)
            .OrderByDescending(l => l.CurrentHighestBid ?? -1))
        {
            var winningBid = lot.Bids
                .OrderByDescending(b => b.Amount)
                .ThenBy(b => b.PlacedAt)
                .FirstOrDefault(b => !cap.HasValue || winCounts.GetValueOrDefault(b.BusinessPartnerId) < cap.Value);

            if (winningBid is not null)
            {
                lot.Status = ProducerPartnershipAuctionLotStatus.Awarded;
                lot.WinningBidId = winningBid.Id;
                winCounts[winningBid.BusinessPartnerId] = winCounts.GetValueOrDefault(winningBid.BusinessPartnerId) + 1;
                awarded.Add((lot.Id, lot.ProducerId, winningBid.BusinessPartnerId, winningBid.Amount));
            }
            else
            {
                lot.Status = ProducerPartnershipAuctionLotStatus.Unsold;
            }

            lot.UpdatedAt = now;
        }

        auction.Status = ProducerPartnershipAuctionStatus.Ended;
        auction.BiddingClosesAt ??= now;
        auction.UpdatedAt = now;
        await _lotRepository.SaveChangesAsync(cancellationToken);

        // Each awarded lot becomes a Pending partnership proposal — never Active yet, since the
        // revenue-share terms still need an admin's input and both parties still need to confirm.
        foreach (var win in awarded)
        {
            await _agreementService.CreateFromAuctionWinAsync(
                auction.Id, win.LotId, win.ProducerId, win.BusinessPartnerId, win.Amount,
                auction.DefaultPartnershipDurationMonths, auction.DefaultRevenueSharePercentage, cancellationToken);
        }
        return ToDto(auction);
    }

    public async Task<ProducerPartnershipAuctionDto> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var auction = await RequireAuctionAsync(id, cancellationToken);

        if (auction.Status is ProducerPartnershipAuctionStatus.Ended or ProducerPartnershipAuctionStatus.Settled or ProducerPartnershipAuctionStatus.Cancelled)
        {
            throw new ConflictException("This auction can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        var lots = await _lotRepository.GetForAuctionAsync(id, cancellationToken);
        foreach (var lot in lots.Where(l => l.Status is ProducerPartnershipAuctionLotStatus.Pending or ProducerPartnershipAuctionLotStatus.Open))
        {
            lot.Status = ProducerPartnershipAuctionLotStatus.Withdrawn;
            lot.UpdatedAt = now;
        }

        auction.Status = ProducerPartnershipAuctionStatus.Cancelled;
        auction.UpdatedAt = now;
        await _lotRepository.SaveChangesAsync(cancellationToken);
        return ToDto(auction);
    }

    private async Task<Domain.Entities.ProducerPartnership.ProducerPartnershipAuction> RequireAuctionAsync(Guid id, CancellationToken cancellationToken)
        => await _auctionRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Producer partnership auction not found.");

    private static void RequireStatus(
        Domain.Entities.ProducerPartnership.ProducerPartnershipAuction auction, ProducerPartnershipAuctionStatus required, string action)
    {
        if (auction.Status != required)
        {
            throw new ConflictException($"The auction must be {required} to be {action} (it is currently {auction.Status}).");
        }
    }
}
