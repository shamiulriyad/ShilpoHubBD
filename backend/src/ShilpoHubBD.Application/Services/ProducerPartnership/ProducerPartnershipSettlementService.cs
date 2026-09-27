using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Application.Services.ProducerPartnership;

public class ProducerPartnershipSettlementService : IProducerPartnershipSettlementService
{
    private readonly IProducerPartnershipSettlementRepository _settlementRepository;
    private readonly IProducerPartnershipAgreementRepository _agreementRepository;
    private readonly IProducerPartnershipAgreementService _agreementService;
    private readonly IProducerOrderRepository _producerOrderRepository;

    public ProducerPartnershipSettlementService(
        IProducerPartnershipSettlementRepository settlementRepository, IProducerPartnershipAgreementRepository agreementRepository,
        IProducerPartnershipAgreementService agreementService, IProducerOrderRepository producerOrderRepository)
    {
        _settlementRepository = settlementRepository;
        _agreementRepository = agreementRepository;
        _agreementService = agreementService;
        _producerOrderRepository = producerOrderRepository;
    }

    public async Task<ProducerPartnershipSettlementDto> GenerateAsync(
        Guid agreementId, GenerateProducerPartnershipSettlementRequest request, CancellationToken cancellationToken)
    {
        if (request.PeriodEnd <= request.PeriodStart)
        {
            throw new ConflictException("The settlement period's end must be after its start.");
        }

        var now = DateTime.UtcNow;
        if (request.PeriodEnd > now)
        {
            throw new ConflictException("Cannot generate a settlement for a period that hasn't ended yet.");
        }

        // Reuses the Part 4 guard: throws unless the agreement is (lazily-expiry-checked) Active
        // right now — this alone stops an expired/cancelled/pending partnership from ever reaching
        // this point, which is what keeps a lapsed partnership from settling future revenue.
        await _agreementService.EnsureCanRecordSettlementAsync(agreementId, cancellationToken);

        var agreement = await _agreementRepository.GetByIdWithDetailsAsync(agreementId, cancellationToken)
            ?? throw new NotFoundException("Producer partnership agreement not found.");

        if (agreement.StartDate.HasValue && request.PeriodStart < agreement.StartDate.Value)
        {
            throw new ConflictException("The settlement period cannot start before the partnership's own start date.");
        }

        if (!agreement.ProducerSharePercentage.HasValue || !agreement.BusinessPartnerSharePercentage.HasValue || !agreement.PlatformFeePercentage.HasValue)
        {
            throw new ConflictException("Set the agreement's revenue-share terms before generating a settlement.");
        }

        if (await _settlementRepository.ExistsNonRejectedOverlappingAsync(agreementId, request.PeriodStart, request.PeriodEnd, cancellationToken))
        {
            throw new ConflictException("A settlement already covers part of this period for this agreement.");
        }

        var items = await _producerOrderRepository.GetByProducerAsync(agreement.ProducerId, request.PeriodStart, request.PeriodEnd, cancellationToken);
        var deliveredItems = items.Where(i => i.ProducerStatus == OrderItemProducerStatus.Delivered).ToList();
        var grossRevenue = deliveredItems.Sum(i => i.LineTotal);
        var orderCount = deliveredItems.Select(i => i.OrderId).Distinct().Count();

        var refundDeductions = await _settlementRepository.GetRefundDeductionsAsync(agreement.ProducerId, request.PeriodStart, request.PeriodEnd, cancellationToken);
        var eligibleRevenue = Math.Max(0, grossRevenue - refundDeductions);

        var platformFeeAmount = Math.Round(eligibleRevenue * agreement.PlatformFeePercentage.Value / 100m, 2);
        var netPartnershipRevenue = eligibleRevenue - platformFeeAmount;
        var producerShareAmount = Math.Round(netPartnershipRevenue * agreement.ProducerSharePercentage.Value / 100m, 2);
        var businessPartnerShareAmount = Math.Round(netPartnershipRevenue * agreement.BusinessPartnerSharePercentage.Value / 100m, 2);

        var settlement = new Domain.Entities.ProducerPartnership.ProducerPartnershipSettlement
        {
            Id = Guid.NewGuid(),
            AgreementId = agreementId,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            GrossRevenue = grossRevenue,
            RefundDeductions = refundDeductions,
            PlatformFeePercentageApplied = agreement.PlatformFeePercentage.Value,
            PlatformFeeAmount = platformFeeAmount,
            NetPartnershipRevenue = netPartnershipRevenue,
            ProducerSharePercentageApplied = agreement.ProducerSharePercentage.Value,
            BusinessPartnerSharePercentageApplied = agreement.BusinessPartnerSharePercentage.Value,
            ProducerShareAmount = producerShareAmount,
            BusinessPartnerShareAmount = businessPartnerShareAmount,
            OrderCount = orderCount,
            BelowMinimumThreshold = agreement.MinimumSettlementAmount.HasValue && businessPartnerShareAmount < agreement.MinimumSettlementAmount.Value,
            Status = ProducerPartnershipSettlementStatus.Draft,
            CalculatedAt = now,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _settlementRepository.AddAsync(settlement, cancellationToken);
        await _settlementRepository.SaveChangesAsync(cancellationToken);

        var created = await _settlementRepository.GetByIdAsync(settlement.Id, cancellationToken)
            ?? throw new NotFoundException("Settlement not found.");
        return ToDto(created);
    }

    public async Task<ProducerPartnershipSettlementDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var settlement = await RequireSettlementAsync(id, cancellationToken);
        EnsurePartyAccess(settlement, currentUserId, isAdmin);
        return ToDto(settlement);
    }

    public async Task<List<ProducerPartnershipSettlementDto>> GetForAgreementAsync(Guid agreementId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        if (!isAdmin)
        {
            var agreement = await _agreementRepository.GetByIdWithDetailsAsync(agreementId, cancellationToken)
                ?? throw new NotFoundException("Producer partnership agreement not found.");
            if (agreement.ProducerId != currentUserId && agreement.BusinessPartnerId != currentUserId)
            {
                throw new UnauthorizedAccessException("You do not have permission to access this agreement's settlements.");
            }
        }

        var settlements = await _settlementRepository.GetForAgreementAsync(agreementId, cancellationToken);
        return settlements.Select(ToDto).ToList();
    }

    public async Task<PagedResult<ProducerPartnershipSettlementDto>> GetPagedAsync(
        ProducerPartnershipSettlementQueryParameters parameters, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _settlementRepository.GetPagedAsync(parameters, cancellationToken);
        return new PagedResult<ProducerPartnershipSettlementDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = parameters.Page,
            PageSize = parameters.PageSize,
        };
    }

    public async Task<ProducerPartnershipSettlementDto> SubmitForApprovalAsync(Guid id, CancellationToken cancellationToken)
    {
        var settlement = await RequireSettlementAsync(id, cancellationToken);
        if (settlement.Status != ProducerPartnershipSettlementStatus.Draft)
        {
            throw new ConflictException("Only a draft settlement can be submitted for approval.");
        }

        settlement.Status = ProducerPartnershipSettlementStatus.PendingApproval;
        settlement.UpdatedAt = DateTime.UtcNow;
        await _settlementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(settlement);
    }

    public async Task<ProducerPartnershipSettlementDto> ApproveAsync(Guid id, Guid approvedByUserId, CancellationToken cancellationToken)
    {
        var settlement = await RequireSettlementAsync(id, cancellationToken);
        if (settlement.Status != ProducerPartnershipSettlementStatus.PendingApproval)
        {
            throw new ConflictException("Only a settlement pending approval can be approved.");
        }

        var now = DateTime.UtcNow;
        settlement.Status = ProducerPartnershipSettlementStatus.Approved;
        settlement.ApprovedByUserId = approvedByUserId;
        settlement.ApprovedAt = now;
        settlement.UpdatedAt = now;
        await _settlementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(settlement);
    }

    public async Task<ProducerPartnershipSettlementDto> RejectAsync(Guid id, RejectProducerPartnershipSettlementRequest request, CancellationToken cancellationToken)
    {
        var settlement = await RequireSettlementAsync(id, cancellationToken);
        if (settlement.Status is not (ProducerPartnershipSettlementStatus.Draft or ProducerPartnershipSettlementStatus.PendingApproval))
        {
            throw new ConflictException("Only a draft or pending settlement can be rejected.");
        }

        settlement.Status = ProducerPartnershipSettlementStatus.Rejected;
        settlement.RejectionReason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim();
        settlement.UpdatedAt = DateTime.UtcNow;
        await _settlementRepository.SaveChangesAsync(cancellationToken);
        return ToDto(settlement);
    }

    private async Task<Domain.Entities.ProducerPartnership.ProducerPartnershipSettlement> RequireSettlementAsync(Guid id, CancellationToken cancellationToken)
        => await _settlementRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Settlement not found.");

    private static void EnsurePartyAccess(Domain.Entities.ProducerPartnership.ProducerPartnershipSettlement settlement, Guid currentUserId, bool isAdmin)
    {
        if (isAdmin) return;
        if (settlement.Agreement.ProducerId != currentUserId && settlement.Agreement.BusinessPartnerId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this settlement.");
        }
    }

    private static ProducerPartnershipSettlementDto ToDto(Domain.Entities.ProducerPartnership.ProducerPartnershipSettlement settlement) => new()
    {
        Id = settlement.Id,
        AgreementId = settlement.AgreementId,
        ProducerName = settlement.Agreement.Producer.FullName,
        BusinessPartnerName = settlement.Agreement.BusinessPartner.FullName,
        PeriodStart = settlement.PeriodStart,
        PeriodEnd = settlement.PeriodEnd,
        GrossRevenue = settlement.GrossRevenue,
        RefundDeductions = settlement.RefundDeductions,
        PlatformFeePercentageApplied = settlement.PlatformFeePercentageApplied,
        PlatformFeeAmount = settlement.PlatformFeeAmount,
        NetPartnershipRevenue = settlement.NetPartnershipRevenue,
        ProducerSharePercentageApplied = settlement.ProducerSharePercentageApplied,
        BusinessPartnerSharePercentageApplied = settlement.BusinessPartnerSharePercentageApplied,
        ProducerShareAmount = settlement.ProducerShareAmount,
        BusinessPartnerShareAmount = settlement.BusinessPartnerShareAmount,
        OrderCount = settlement.OrderCount,
        BelowMinimumThreshold = settlement.BelowMinimumThreshold,
        Status = settlement.Status,
        PayoutReference = settlement.PayoutReference,
        ApprovedAt = settlement.ApprovedAt,
        ApprovedByName = settlement.ApprovedBy?.FullName,
        RejectionReason = settlement.RejectionReason,
        CalculatedAt = settlement.CalculatedAt,
        CreatedAt = settlement.CreatedAt,
        UpdatedAt = settlement.UpdatedAt,
    };
}
