using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.CSRSponsorship;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;

namespace ShilpoHubBD.Data.Repositories;

public class CSRSponsorshipRepository : ICSRSponsorshipRepository
{
    private readonly ShilpoHubDbContext _context;

    public CSRSponsorshipRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<SponsorshipOpportunity> OpportunityWithDetails()
        => _context.SponsorshipOpportunities
            .Include(o => o.Producer)
            .Include(o => o.Proposals)
            .AsSplitQuery();

    public Task<SponsorshipOpportunity?> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken)
        => OpportunityWithDetails().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<(List<SponsorshipOpportunity> Items, int TotalCount)> GetPagedOpportunitiesAsync(
        OpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = OpportunityWithDetails();
        if (parameters.Status.HasValue)
        {
            query = query.Where(o => o.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((parameters.Page - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(List<SponsorshipOpportunity> Items, int TotalCount)> GetPagedOpportunitiesForProducerAsync(
        Guid producerId, OpportunityQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = OpportunityWithDetails().Where(o => o.ProducerId == producerId);
        if (parameters.Status.HasValue)
        {
            query = query.Where(o => o.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((parameters.Page - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddOpportunityAsync(SponsorshipOpportunity opportunity, CancellationToken cancellationToken)
        => await _context.SponsorshipOpportunities.AddAsync(opportunity, cancellationToken);

    private IQueryable<SponsorshipProposal> ProposalWithDetails()
        => _context.SponsorshipProposals
            .Include(p => p.Opportunity).ThenInclude(o => o.Producer)
            .Include(p => p.BusinessPartner)
            .Include(p => p.Milestones)
            .Include(p => p.ProgressUpdates).ThenInclude(u => u.Author)
            .Include(p => p.ImpactRecords)
            .Include(p => p.StatusHistory)
            .AsSplitQuery();

    public Task<SponsorshipProposal?> GetProposalByIdAsync(Guid id, CancellationToken cancellationToken)
        => ProposalWithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(List<SponsorshipProposal> Items, int TotalCount)> GetPagedProposalsForBusinessPartnerAsync(
        Guid businessPartnerId, ProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.SponsorshipProposals
            .Include(p => p.Opportunity)
            .Include(p => p.BusinessPartner)
            .Where(p => p.BusinessPartnerId == businessPartnerId)
            .AsSplitQuery();

        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((parameters.Page - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task<(List<SponsorshipProposal> Items, int TotalCount)> GetPagedProposalsForOpportunityAsync(
        Guid opportunityId, ProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.SponsorshipProposals
            .Include(p => p.Opportunity)
            .Include(p => p.BusinessPartner)
            .Where(p => p.OpportunityId == opportunityId)
            .AsSplitQuery();

        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query.Skip((parameters.Page - 1) * parameters.PageSize).Take(parameters.PageSize).ToListAsync(cancellationToken);
        return (items, totalCount);
    }

    public async Task AddProposalAsync(SponsorshipProposal proposal, CancellationToken cancellationToken)
        => await _context.SponsorshipProposals.AddAsync(proposal, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
