using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.PerformingArts;

namespace ShilpoHubBD.Data.Repositories;

public class PerformingArtRepository : IPerformingArtRepository
{
    private readonly ShilpoHubDbContext _context;

    public PerformingArtRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<PerformingArtRecord> Items, int TotalCount)> SearchAsync(
        Expression<Func<PerformingArtRecord, bool>> filter, int page, int pageSize, CancellationToken cancellationToken)
    {
        var query = _context.PerformingArtRecords.AsNoTracking().Where(filter);
        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .Include(r => r.District)
            .Include(r => r.Media)
            .Include(r => r.Sources)
            .OrderByDescending(r => r.IsFeatured).ThenBy(r => r.DisplayOrder).ThenBy(r => r.TitleEn)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return (items, total);
    }

    public Task<List<PerformingArtRecord>> ListAsync(Expression<Func<PerformingArtRecord, bool>> filter, CancellationToken cancellationToken)
        => _context.PerformingArtRecords.AsNoTracking()
            .Where(filter)
            .Include(r => r.District)
            .Include(r => r.Sources)
            .OrderBy(r => r.DisplayOrder).ThenBy(r => r.TitleEn)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

    public Task<PerformingArtRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithChildren().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<PerformingArtRecord?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => WithChildren().FirstOrDefaultAsync(r => r.Slug == slug, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken)
        => _context.PerformingArtRecords.AnyAsync(r => r.Slug == slug && (exceptId == null || r.Id != exceptId), cancellationToken);

    public Task<District?> GetDistrictAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.FirstOrDefaultAsync(d => d.Id == districtId, cancellationToken);

    public async Task AddAsync(PerformingArtRecord record, CancellationToken cancellationToken)
        => await _context.PerformingArtRecords.AddAsync(record, cancellationToken);

    public void Remove(PerformingArtRecord record) => _context.PerformingArtRecords.Remove(record);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);

    private IQueryable<PerformingArtRecord> WithChildren()
        => _context.PerformingArtRecords
            .Include(r => r.District)
            .Include(r => r.Sources)
            .Include(r => r.Media)
            .AsSplitQuery();
}
