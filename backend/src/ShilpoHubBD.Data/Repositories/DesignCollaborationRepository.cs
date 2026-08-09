using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.DesignCollaboration;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;

namespace ShilpoHubBD.Data.Repositories;

public class DesignCollaborationRepository : IDesignCollaborationRepository
{
    private readonly ShilpoHubDbContext _context;

    public DesignCollaborationRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<DesignCollaborationProject> WithDetails()
        => _context.DesignCollaborationProjects
            .Include(p => p.BusinessPartner)
            .Include(p => p.Producer)
            .Include(p => p.Files).ThenInclude(f => f.UploadedBy)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Revisions).ThenInclude(r => r.SubmittedBy)
            .Include(p => p.Revisions).ThenInclude(r => r.Files)
            .Include(p => p.StatusHistory)
            .AsSplitQuery();

    private IQueryable<DesignCollaborationProject> ForListing()
        => _context.DesignCollaborationProjects
            .Include(p => p.Producer)
            .Include(p => p.Revisions)
            .AsSplitQuery();

    public Task<DesignCollaborationProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    private static async Task<(List<DesignCollaborationProject> Items, int TotalCount)> PageAsync(
        IQueryable<DesignCollaborationProject> query, ProjectQueryParameters parameters, CancellationToken cancellationToken)
    {
        if (parameters.Status.HasValue)
        {
            query = query.Where(p => p.Status == parameters.Status.Value);
        }

        query = query.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .Skip((parameters.Page - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<(List<DesignCollaborationProject> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, ProjectQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<DesignCollaborationProject> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, ProjectQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.ProducerId == producerId), parameters, cancellationToken);

    public Task<(List<DesignCollaborationProject> Items, int TotalCount)> GetPagedAllAsync(
        ProjectQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        => _context.DesignCollaborationProjects.AnyAsync(p => p.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task AddAsync(DesignCollaborationProject project, CancellationToken cancellationToken)
        => await _context.DesignCollaborationProjects.AddAsync(project, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
