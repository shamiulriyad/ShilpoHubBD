using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProducerPartnership;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ProducerPartnership;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerPartnershipAgreementRepository : IProducerPartnershipAgreementRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerPartnershipAgreementRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProducerPartnershipAgreement> WithDetails()
        => _context.ProducerPartnershipAgreements
            .Include(g => g.Producer)
            .Include(g => g.BusinessPartner)
            .Include(g => g.Auction)
            .Include(g => g.AuctionLot)
            .Include(g => g.StatusHistory)
            .AsSplitQuery();

    private IQueryable<ProducerPartnershipAgreement> ForListing()
        => _context.ProducerPartnershipAgreements
            .Include(g => g.Producer)
            .Include(g => g.BusinessPartner)
            .AsSplitQuery();

    public Task<ProducerPartnershipAgreement?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(g => g.Id == id, cancellationToken);

    private static async Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> PageAsync(
        IQueryable<ProducerPartnershipAgreement> query, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(parameters.Status)
            && Enum.TryParse<ProducerPartnershipAgreementStatus>(parameters.Status, true, out var status))
        {
            query = query.Where(g => g.Status == status);
        }

        query = query.OrderByDescending(g => g.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(g => g.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(g => g.ProducerId == producerId), parameters, cancellationToken);

    public Task<(List<ProducerPartnershipAgreement> Items, int TotalCount)> GetPagedAllAsync(
        ProducerPartnershipAgreementQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public async Task AddAsync(ProducerPartnershipAgreement agreement, CancellationToken cancellationToken)
        => await _context.ProducerPartnershipAgreements.AddAsync(agreement, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    public Task<bool> HasActiveAgreementAsync(Guid producerId, Guid? businessPartnerId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAgreements.AnyAsync(g =>
            g.ProducerId == producerId
            && g.Status == ProducerPartnershipAgreementStatus.Active
            && (businessPartnerId == null || g.BusinessPartnerId == businessPartnerId.Value),
            cancellationToken);

    public Task<bool> ExistsForAuctionLotAsync(Guid auctionLotId, CancellationToken cancellationToken)
        => _context.ProducerPartnershipAgreements.AnyAsync(g => g.AuctionLotId == auctionLotId, cancellationToken);
}
