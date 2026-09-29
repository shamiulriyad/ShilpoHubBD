using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Logistics;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerPartnershipSettlementRepository : IProducerPartnershipSettlementRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerPartnershipSettlementRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProducerPartnershipSettlement> WithDetails()
        => _context.ProducerPartnershipSettlements
            .Include(s => s.Agreement).ThenInclude(a => a.Producer)
            .Include(s => s.Agreement).ThenInclude(a => a.BusinessPartner)
            .Include(s => s.ApprovedBy)
            .AsSplitQuery();

    public Task<ProducerPartnershipSettlement?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<List<ProducerPartnershipSettlement>> GetForAgreementAsync(Guid agreementId, CancellationToken cancellationToken)
        => WithDetails().Where(s => s.AgreementId == agreementId).OrderByDescending(s => s.PeriodStart).ToListAsync(cancellationToken);

    public async Task<(List<ProducerPartnershipSettlement> Items, int TotalCount)> GetPagedAsync(
        ProducerPartnershipSettlementQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = WithDetails();

        if (parameters.AgreementId.HasValue)
        {
            query = query.Where(s => s.AgreementId == parameters.AgreementId.Value);
        }

        if (!string.IsNullOrWhiteSpace(parameters.Status)
            && Enum.TryParse<ProducerPartnershipSettlementStatus>(parameters.Status, true, out var status))
        {
            query = query.Where(s => s.Status == status);
        }

        query = query.OrderByDescending(s => s.PeriodStart);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<bool> ExistsNonRejectedOverlappingAsync(Guid agreementId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken)
        => _context.ProducerPartnershipSettlements.AnyAsync(s =>
            s.AgreementId == agreementId
            && s.Status != ProducerPartnershipSettlementStatus.Rejected
            && s.PeriodStart < periodEnd && s.PeriodEnd > periodStart,
            cancellationToken);

    public async Task AddAsync(ProducerPartnershipSettlement settlement, CancellationToken cancellationToken)
        => await _context.ProducerPartnershipSettlements.AddAsync(settlement, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    public async Task<PlatformRevenueSummaryDto> GetApprovedTotalsAsync(CancellationToken cancellationToken)
    {
        var approved = await _context.ProducerPartnershipSettlements
            .Where(s => s.Status == ProducerPartnershipSettlementStatus.Approved)
            .Select(s => new { s.PlatformFeeAmount, s.GrossRevenue, s.NetPartnershipRevenue })
            .ToListAsync(cancellationToken);

        return new PlatformRevenueSummaryDto
        {
            TotalPlatformFee = approved.Sum(s => s.PlatformFeeAmount),
            TotalGrossRevenue = approved.Sum(s => s.GrossRevenue),
            TotalNetPartnershipRevenue = approved.Sum(s => s.NetPartnershipRevenue),
            ApprovedSettlementCount = approved.Count,
        };
    }

    public async Task<decimal> GetRefundDeductionsAsync(Guid producerId, DateTime periodStart, DateTime periodEnd, CancellationToken cancellationToken)
    {
        var returnItems = await _context.Set<ReturnItem>()
            .Include(ri => ri.ReturnRequest)
            .Include(ri => ri.Product)
            .Where(ri => ri.Product != null && ri.Product.ProducerId == producerId
                && ri.ReturnRequest.Status == ReturnStatus.Refunded
                && ri.ReturnRequest.RefundedAt != null
                && ri.ReturnRequest.RefundedAt >= periodStart && ri.ReturnRequest.RefundedAt <= periodEnd)
            .ToListAsync(cancellationToken);

        return returnItems.Sum(ri => (ri.UnitRefundAmount ?? 0m) * ri.Quantity);
    }
}
