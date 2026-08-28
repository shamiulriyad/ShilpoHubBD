using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Investment;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Investment;

namespace ShilpoHubBD.Data.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly ShilpoHubDbContext _context;

    public InvestmentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<InvestmentOpportunity> OpportunityWithDetails()
        => _context.InvestmentOpportunities
            .Include(o => o.Producer)
            .Include(o => o.Proposals)
            .AsSplitQuery();

    public Task<InvestmentOpportunity?> GetOpportunityByIdAsync(Guid id, CancellationToken cancellationToken)
        => OpportunityWithDetails().FirstOrDefaultAsync(o => o.Id == id, cancellationToken);

    public async Task<(List<InvestmentOpportunity> Items, int TotalCount)> GetPagedOpportunitiesAsync(
        InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken)
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

    public async Task<(List<InvestmentOpportunity> Items, int TotalCount)> GetPagedOpportunitiesForProducerAsync(
        Guid producerId, InvestmentOpportunityQueryParameters parameters, CancellationToken cancellationToken)
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

    public async Task AddOpportunityAsync(InvestmentOpportunity opportunity, CancellationToken cancellationToken)
        => await _context.InvestmentOpportunities.AddAsync(opportunity, cancellationToken);

    private IQueryable<InvestmentProposal> ProposalWithDetails()
        => _context.InvestmentProposals
            .Include(p => p.Opportunity).ThenInclude(o => o.Producer)
            .Include(p => p.BusinessPartner)
            .Include(p => p.Milestones)
            .Include(p => p.Documents)
            .Include(p => p.StatusHistory)
            .AsSplitQuery();

    public Task<InvestmentProposal?> GetProposalByIdAsync(Guid id, CancellationToken cancellationToken)
        => ProposalWithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<(List<InvestmentProposal> Items, int TotalCount)> GetPagedProposalsForBusinessPartnerAsync(
        Guid businessPartnerId, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.InvestmentProposals
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

    public async Task<(List<InvestmentProposal> Items, int TotalCount)> GetPagedProposalsForOpportunityAsync(
        Guid opportunityId, InvestmentProposalQueryParameters parameters, CancellationToken cancellationToken)
    {
        var query = _context.InvestmentProposals
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

    public async Task AddProposalAsync(InvestmentProposal proposal, CancellationToken cancellationToken)
        => await _context.InvestmentProposals.AddAsync(proposal, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
