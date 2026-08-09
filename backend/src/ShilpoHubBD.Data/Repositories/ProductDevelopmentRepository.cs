using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProductDevelopment;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;

namespace ShilpoHubBD.Data.Repositories;

public class ProductDevelopmentRepository : IProductDevelopmentRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProductDevelopmentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ProductDevelopmentProject> WithDetails()
        => _context.ProductDevelopmentProjects
            .Include(p => p.BusinessPartner)
            .Include(p => p.Producer)
            .Include(p => p.FinalProduct)
            .Include(p => p.PrototypeVersions).ThenInclude(v => v.SubmittedBy)
            .Include(p => p.PrototypeVersions).ThenInclude(v => v.Files)
            .Include(p => p.Comments).ThenInclude(c => c.Author)
            .Include(p => p.Milestones)
            .Include(p => p.StatusHistory)
            .AsSplitQuery();

    private IQueryable<ProductDevelopmentProject> ForListing()
        => _context.ProductDevelopmentProjects
            .Include(p => p.Producer)
            .Include(p => p.PrototypeVersions)
            .AsSplitQuery();

    public Task<ProductDevelopmentProject?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    private static async Task<(List<ProductDevelopmentProject> Items, int TotalCount)> PageAsync(
        IQueryable<ProductDevelopmentProject> query, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
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

    public Task<(List<ProductDevelopmentProject> Items, int TotalCount)> GetPagedForBusinessPartnerAsync(
        Guid businessPartnerId, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.BusinessPartnerId == businessPartnerId), parameters, cancellationToken);

    public Task<(List<ProductDevelopmentProject> Items, int TotalCount)> GetPagedForProducerAsync(
        Guid producerId, DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing().Where(p => p.ProducerId == producerId), parameters, cancellationToken);

    public Task<(List<ProductDevelopmentProject> Items, int TotalCount)> GetPagedAllAsync(
        DevelopmentProjectQueryParameters parameters, CancellationToken cancellationToken)
        => PageAsync(ForListing(), parameters, cancellationToken);

    public Task<bool> ExistsByReferenceNumberAsync(string referenceNumber, CancellationToken cancellationToken)
        => _context.ProductDevelopmentProjects.AnyAsync(p => p.ReferenceNumber == referenceNumber, cancellationToken);

    public async Task AddAsync(ProductDevelopmentProject project, CancellationToken cancellationToken)
        => await _context.ProductDevelopmentProjects.AddAsync(project, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
