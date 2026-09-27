using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Services.ProducerPartnership;

public class ProducerPartnershipAgreementService : IProducerPartnershipAgreementService
{
    private readonly IProducerPartnershipAgreementRepository _agreementRepository;
    private readonly IUserRepository _userRepository;

    public ProducerPartnershipAgreementService(
        IProducerPartnershipAgreementRepository agreementRepository, IUserRepository userRepository)
    {
        _agreementRepository = agreementRepository;
        _userRepository = userRepository;
    }

    public async Task<ProducerPartnershipAgreementDto> CreateAsync(
        Guid createdByUserId, CreateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken)
    {
        var producer = await _userRepository.GetByIdWithRolesAsync(request.ProducerId, cancellationToken);
        if (producer is null || !producer.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
        {
            throw new NotFoundException("Producer not found.");
        }

        var businessPartner = await _userRepository.GetByIdWithRolesAsync(request.BusinessPartnerId, cancellationToken);
        if (businessPartner is null || !businessPartner.UserRoles.Any(ur => ur.Role.Name == RoleNames.BusinessPartner))
        {
            throw new NotFoundException("Business partner not found.");
        }

        if (request.EndDate.HasValue && request.StartDate.HasValue && request.EndDate <= request.StartDate)
        {
            throw new ConflictException("End date must be after the start date.");
        }

        var now = DateTime.UtcNow;
        var agreement = new Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement
        {
            Id = Guid.NewGuid(),
            AuctionId = request.AuctionId,
            ProducerId = request.ProducerId,
            BusinessPartnerId = request.BusinessPartnerId,
            Status = ProducerPartnershipAgreementStatus.Pending,
            WinningBidAmount = request.WinningBidAmount,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            PartnershipDurationMonths = request.PartnershipDurationMonths,
            AgreementTerms = string.IsNullOrWhiteSpace(request.AgreementTerms) ? null : request.AgreementTerms.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.Pending,
            Note = "Agreement created by admin.",
            ChangedByUserId = createdByUserId,
            CreatedAt = now,
        });

        await _agreementRepository.AddAsync(agreement, cancellationToken);
        await _agreementRepository.SaveChangesAsync(cancellationToken);

        var created = await _agreementRepository.GetByIdWithDetailsAsync(agreement.Id, cancellationToken)
            ?? throw new NotFoundException("Producer partnership agreement not found.");
        return ToDto(created);
    }

    public async Task CreateFromAuctionWinAsync(
        Guid auctionId, Guid auctionLotId, Guid producerId, Guid businessPartnerId, decimal winningBidAmount,
        int defaultPartnershipDurationMonths, decimal? defaultProducerSharePercentage, CancellationToken cancellationToken)
    {
        if (await _agreementRepository.ExistsForAuctionLotAsync(auctionLotId, cancellationToken))
        {
            return;
        }

        var now = DateTime.UtcNow;
        var agreement = new Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement
        {
            Id = Guid.NewGuid(),
            AuctionId = auctionId,
            AuctionLotId = auctionLotId,
            ProducerId = producerId,
            BusinessPartnerId = businessPartnerId,
            Status = ProducerPartnershipAgreementStatus.Pending,
            WinningBidAmount = winningBidAmount,
            PartnershipDurationMonths = defaultPartnershipDurationMonths,
            // A default suggestion only — the bid amount is never assumed to be the revenue share,
            // and BusinessPartnerSharePercentage/PlatformFeePercentage are left for the admin to set.
            ProducerSharePercentage = defaultProducerSharePercentage,
            CreatedAt = now,
            UpdatedAt = now,
        };

        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.Pending,
            Note = $"Created from auction win — winning bid {winningBidAmount}.",
            CreatedAt = now,
        });

        await _agreementRepository.AddAsync(agreement, cancellationToken);
        await _agreementRepository.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<ProducerPartnershipAgreementListItemDto>> GetForBusinessPartnerAsync(
        Guid businessPartnerId, bool isAdmin, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = isAdmin
            ? await _agreementRepository.GetPagedAllAsync(parameters, cancellationToken)
            : await _agreementRepository.GetPagedForBusinessPartnerAsync(businessPartnerId, parameters, cancellationToken);

        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<PagedResult<ProducerPartnershipAgreementListItemDto>> GetForProducerAsync(
        Guid producerId, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _agreementRepository.GetPagedForProducerAsync(producerId, parameters, cancellationToken);
        return ToPagedListDto(items, totalCount, parameters);
    }

    public async Task<ProducerPartnershipAgreementDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var agreement = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);
        return ToDto(agreement);
    }

    public async Task<ProducerPartnershipAgreementDto> UpdateTermsAsync(
        Guid id, UpdateProducerPartnershipAgreementTermsRequest request, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);
        if (agreement.Status != ProducerPartnershipAgreementStatus.Pending)
        {
            throw new ConflictException("Terms can only be edited while the agreement is Pending.");
        }

        if (request.StartDate.HasValue) agreement.StartDate = request.StartDate;
        if (request.EndDate.HasValue) agreement.EndDate = request.EndDate;
        if (agreement.EndDate.HasValue && agreement.StartDate.HasValue && agreement.EndDate <= agreement.StartDate)
        {
            throw new ConflictException("End date must be after the start date.");
        }

        if (request.PartnershipDurationMonths.HasValue) agreement.PartnershipDurationMonths = request.PartnershipDurationMonths;
        if (request.ProducerSharePercentage.HasValue) agreement.ProducerSharePercentage = request.ProducerSharePercentage;
        if (request.BusinessPartnerSharePercentage.HasValue) agreement.BusinessPartnerSharePercentage = request.BusinessPartnerSharePercentage;
        if (request.PlatformFeePercentage.HasValue) agreement.PlatformFeePercentage = request.PlatformFeePercentage;
        if (request.CustomSettlementPeriodDays.HasValue) agreement.CustomSettlementPeriodDays = request.CustomSettlementPeriodDays;
        if (request.MinimumSettlementAmount.HasValue) agreement.MinimumSettlementAmount = request.MinimumSettlementAmount;

        // The platform fee is taken off the top of gross revenue first; Producer share and BP share
        // then split what's left, so only those two need to sum to 100% — the fee is a separate,
        // independent percentage of gross, not a third co-equal share of the same pie.
        if (agreement.ProducerSharePercentage.HasValue && agreement.BusinessPartnerSharePercentage.HasValue)
        {
            var sum = agreement.ProducerSharePercentage.Value + agreement.BusinessPartnerSharePercentage.Value;
            if (Math.Abs(sum - 100m) > 0.01m)
            {
                throw new ConflictException($"Producer share + Business Partner share must total 100% (currently {sum}%). The platform fee is a separate percentage taken from gross revenue.");
            }
        }

        if (!string.IsNullOrWhiteSpace(request.SettlementFrequency))
        {
            agreement.SettlementFrequency = Enum.TryParse<ProducerPartnershipSettlementFrequency>(request.SettlementFrequency, true, out var frequency)
                ? frequency
                : throw new ConflictException($"'{request.SettlementFrequency}' is not a valid settlement frequency.");
        }

        if (request.AgreementTerms is not null)
        {
            agreement.AgreementTerms = string.IsNullOrWhiteSpace(request.AgreementTerms) ? null : request.AgreementTerms.Trim();
        }

        agreement.UpdatedAt = DateTime.UtcNow;
        await _agreementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(agreement);
    }

    public async Task<ProducerPartnershipAgreementDto> SubmitForConfirmationAsync(Guid id, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);
        if (agreement.Status != ProducerPartnershipAgreementStatus.Pending)
        {
            throw new ConflictException("Only a pending agreement can be submitted for confirmation.");
        }

        RequireCompleteTerms(agreement);

        var now = DateTime.UtcNow;
        agreement.Status = ProducerPartnershipAgreementStatus.AwaitingProducerConfirmation;
        agreement.UpdatedAt = now;
        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.AwaitingProducerConfirmation,
            Note = "Terms submitted — awaiting producer confirmation.",
            CreatedAt = now,
        });

        await _agreementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(agreement);
    }

    public async Task<ProducerPartnershipAgreementDto> ConfirmAsync(Guid id, Guid currentUserId, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);
        var now = DateTime.UtcNow;

        if (agreement.Status == ProducerPartnershipAgreementStatus.AwaitingProducerConfirmation)
        {
            if (agreement.ProducerId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the producer can confirm this agreement at its current stage.");
            }

            agreement.ProducerConfirmedAt = now;
            agreement.Status = ProducerPartnershipAgreementStatus.AwaitingBPConfirmation;
            agreement.UpdatedAt = now;
            agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
            {
                Id = Guid.NewGuid(),
                Status = ProducerPartnershipAgreementStatus.AwaitingBPConfirmation,
                Note = "Producer confirmed — awaiting Business Partner confirmation.",
                ChangedByUserId = currentUserId,
                CreatedAt = now,
            });

            await _agreementRepository.SaveChangesAsync(cancellationToken);
            return ToDto(agreement);
        }

        if (agreement.Status == ProducerPartnershipAgreementStatus.AwaitingBPConfirmation)
        {
            if (agreement.BusinessPartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("Only the Business Partner can confirm this agreement at its current stage.");
            }

            // Re-check every activation condition at the moment of the second confirmation — not
            // just at submission time — since time has passed and state may have changed.
            RequireCompleteTerms(agreement);
            if (await _agreementRepository.HasActiveAgreementAsync(agreement.ProducerId, null, cancellationToken))
            {
                throw new ConflictException("This producer already has another active partnership. Resolve or end it before activating this one.");
            }

            agreement.BusinessPartnerConfirmedAt = now;
            agreement.StartDate ??= now;
            if (!agreement.EndDate.HasValue && agreement.PartnershipDurationMonths.HasValue)
            {
                agreement.EndDate = agreement.StartDate.Value.AddMonths(agreement.PartnershipDurationMonths.Value);
            }

            agreement.Status = ProducerPartnershipAgreementStatus.Active;
            agreement.UpdatedAt = now;
            agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
            {
                Id = Guid.NewGuid(),
                Status = ProducerPartnershipAgreementStatus.Active,
                Note = "Both parties confirmed — partnership activated.",
                ChangedByUserId = currentUserId,
                CreatedAt = now,
            });

            await _agreementRepository.SaveChangesAsync(cancellationToken);
            return ToDto(agreement);
        }

        throw new ConflictException("This agreement is not currently awaiting confirmation.");
    }

    public async Task<ProducerPartnershipAgreementDto> SuspendAsync(Guid id, string? reason, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);
        if (agreement.Status != ProducerPartnershipAgreementStatus.Active)
        {
            throw new ConflictException("Only an active partnership can be suspended.");
        }

        var now = DateTime.UtcNow;
        var note = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim();
        agreement.Status = ProducerPartnershipAgreementStatus.Suspended;
        agreement.EndReason = note;
        agreement.UpdatedAt = now;
        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.Suspended,
            Note = note,
            CreatedAt = now,
        });

        await _agreementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(agreement);
    }

    public async Task<ProducerPartnershipAgreementDto> ResumeAsync(Guid id, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);
        if (agreement.Status != ProducerPartnershipAgreementStatus.Suspended)
        {
            throw new ConflictException("Only a suspended partnership can be resumed.");
        }

        if (await _agreementRepository.HasActiveAgreementAsync(agreement.ProducerId, null, cancellationToken))
        {
            throw new ConflictException("This producer already has another active partnership. Resolve or end it before resuming this one.");
        }

        var now = DateTime.UtcNow;
        agreement.Status = ProducerPartnershipAgreementStatus.Active;
        agreement.EndReason = null;
        agreement.UpdatedAt = now;
        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.Active,
            Note = "Partnership resumed.",
            CreatedAt = now,
        });

        await _agreementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(agreement);
    }

    public async Task<ProducerPartnershipAgreementDto> CompleteAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var agreement = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);
        if (agreement.Status != ProducerPartnershipAgreementStatus.Active)
        {
            throw new ConflictException("Only an active partnership can be marked as completed.");
        }

        var now = DateTime.UtcNow;
        agreement.Status = ProducerPartnershipAgreementStatus.Completed;
        agreement.EndedAt = now;
        agreement.UpdatedAt = now;
        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.Completed,
            Note = "Partnership completed.",
            ChangedByUserId = currentUserId,
            CreatedAt = now,
        });

        await _agreementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(agreement);
    }

    public async Task<ProducerPartnershipAgreementDto> CancelAsync(
        Guid id, Guid currentUserId, bool isAdmin, TerminateProducerPartnershipAgreementRequest request, CancellationToken cancellationToken)
    {
        var agreement = await GetPartyAsync(id, currentUserId, isAdmin, cancellationToken);

        if (agreement.Status is ProducerPartnershipAgreementStatus.Completed
            or ProducerPartnershipAgreementStatus.Cancelled
            or ProducerPartnershipAgreementStatus.Expired)
        {
            throw new ConflictException("This partnership can no longer be cancelled.");
        }

        var now = DateTime.UtcNow;
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();

        agreement.Status = ProducerPartnershipAgreementStatus.Cancelled;
        agreement.EndedAt = now;
        agreement.EndReason = reason;
        agreement.UpdatedAt = now;
        agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
        {
            Id = Guid.NewGuid(),
            Status = ProducerPartnershipAgreementStatus.Cancelled,
            Note = reason,
            ChangedByUserId = currentUserId,
            CreatedAt = now,
        });

        await _agreementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(agreement);
    }

    public async Task EnsureCanRecordSettlementAsync(Guid id, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);
        if (agreement.Status != ProducerPartnershipAgreementStatus.Active)
        {
            throw new ConflictException($"Cannot record a settlement for this partnership — it is {agreement.Status}, not Active.");
        }
    }

    private static void RequireCompleteTerms(Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement agreement)
    {
        if (!agreement.ProducerSharePercentage.HasValue || !agreement.BusinessPartnerSharePercentage.HasValue || !agreement.PlatformFeePercentage.HasValue)
        {
            throw new ConflictException("Set the producer share, Business Partner share and platform fee percentages before proceeding.");
        }

        var sum = agreement.ProducerSharePercentage.Value + agreement.BusinessPartnerSharePercentage.Value;
        if (Math.Abs(sum - 100m) > 0.01m)
        {
            throw new ConflictException($"Producer share + Business Partner share must total 100% (currently {sum}%). The platform fee is a separate percentage taken from gross revenue.");
        }

        if (!agreement.SettlementFrequency.HasValue)
        {
            throw new ConflictException("Set a settlement frequency before proceeding.");
        }
    }

    /// <summary>Fetches the agreement and lazily expires it in place if its end date has passed while still Active.</summary>
    private async Task<Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement> RequireAgreementAsync(Guid id, CancellationToken cancellationToken)
    {
        var agreement = await _agreementRepository.GetByIdWithDetailsAsync(id, cancellationToken)
            ?? throw new NotFoundException("Producer partnership agreement not found.");

        if (agreement.Status == ProducerPartnershipAgreementStatus.Active
            && agreement.EndDate.HasValue && DateTime.UtcNow > agreement.EndDate.Value)
        {
            var now = DateTime.UtcNow;
            agreement.Status = ProducerPartnershipAgreementStatus.Expired;
            agreement.EndedAt = now;
            agreement.UpdatedAt = now;
            agreement.StatusHistory.Add(new ProducerPartnershipStatusEvent
            {
                Id = Guid.NewGuid(),
                Status = ProducerPartnershipAgreementStatus.Expired,
                Note = "End date passed.",
                CreatedAt = now,
            });
            await _agreementRepository.SaveChangesAsync(cancellationToken);
        }

        return agreement;
    }

    private async Task<Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement> GetPartyAsync(
        Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var agreement = await RequireAgreementAsync(id, cancellationToken);

        if (!isAdmin && agreement.BusinessPartnerId != currentUserId && agreement.ProducerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this partnership agreement.");
        }

        return agreement;
    }

    private static PagedResult<ProducerPartnershipAgreementListItemDto> ToPagedListDto(
        List<Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement> items, int totalCount, ProducerPartnershipAgreementQueryParameters parameters)
    {
        return new PagedResult<ProducerPartnershipAgreementListItemDto>
        {
            Items = items.Select(g => new ProducerPartnershipAgreementListItemDto
            {
                Id = g.Id,
                ProducerName = g.Producer.FullName,
                BusinessPartnerName = g.BusinessPartner.FullName,
                Status = g.Status,
                WinningBidAmount = g.WinningBidAmount,
                StartDate = g.StartDate,
                EndDate = g.EndDate,
                ProducerSharePercentage = g.ProducerSharePercentage,
                BusinessPartnerSharePercentage = g.BusinessPartnerSharePercentage,
                CreatedAt = g.CreatedAt,
            }).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    private static ProducerPartnershipAgreementDto ToDto(Domain.Entities.ProducerPartnership.ProducerPartnershipAgreement agreement) => new()
    {
        Id = agreement.Id,
        AuctionId = agreement.AuctionId,
        AuctionName = agreement.Auction?.Name,
        AuctionLotId = agreement.AuctionLotId,
        ProducerId = agreement.ProducerId,
        ProducerName = agreement.Producer.FullName,
        BusinessPartnerId = agreement.BusinessPartnerId,
        BusinessPartnerName = agreement.BusinessPartner.FullName,
        Status = agreement.Status,
        WinningBidAmount = agreement.WinningBidAmount,
        StartDate = agreement.StartDate,
        EndDate = agreement.EndDate,
        PartnershipDurationMonths = agreement.PartnershipDurationMonths,
        ProducerSharePercentage = agreement.ProducerSharePercentage,
        BusinessPartnerSharePercentage = agreement.BusinessPartnerSharePercentage,
        PlatformFeePercentage = agreement.PlatformFeePercentage,
        SettlementFrequency = agreement.SettlementFrequency,
        CustomSettlementPeriodDays = agreement.CustomSettlementPeriodDays,
        MinimumSettlementAmount = agreement.MinimumSettlementAmount,
        AgreementTerms = agreement.AgreementTerms,
        ProducerConfirmedAt = agreement.ProducerConfirmedAt,
        BusinessPartnerConfirmedAt = agreement.BusinessPartnerConfirmedAt,
        EndedAt = agreement.EndedAt,
        EndReason = agreement.EndReason,
        StatusHistory = agreement.StatusHistory
            .OrderByDescending(h => h.CreatedAt)
            .Select(h => new ProducerPartnershipStatusEventDto
            {
                Status = h.Status,
                Note = h.Note,
                CreatedAt = h.CreatedAt,
            }).ToList(),
        CreatedAt = agreement.CreatedAt,
        UpdatedAt = agreement.UpdatedAt,
    };
}
