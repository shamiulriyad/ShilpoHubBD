using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageDatasetRepository : IHeritageDatasetRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageDatasetRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Datasets ----------------------------------------------------------

    public Task<HeritageDataset?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.HeritageDatasets
            .Include(d => d.Owner)
            .Include(d => d.AccessGrants)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<HeritageDataset?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        => _context.HeritageDatasets
            .Include(d => d.Owner)
            .Include(d => d.Versions).ThenInclude(v => v.CreatedBy)
            .Include(d => d.AccessGrants).ThenInclude(g => g.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
        => _context.HeritageDatasets.AnyAsync(d => d.Slug == slug, cancellationToken);

    public async Task<(List<HeritageDataset> Items, int TotalCount)> GetPagedAccessibleAsync(
        Guid userId, bool canSeeResearcherLevel, HeritageDatasetQueryParameters query, CancellationToken cancellationToken)
    {
        var datasets = _context.HeritageDatasets
            .Include(d => d.Owner)
            .Include(d => d.AccessGrants)
            .Where(d =>
                d.OwnerUserId == userId
                || d.AccessGrants.Any(g => g.UserId == userId)
                || d.AccessLevel == HeritageDatasetAccessLevel.Public
                || (canSeeResearcherLevel && d.AccessLevel == HeritageDatasetAccessLevel.Researcher));

        // Non-owners never see drafts.
        datasets = datasets.Where(d => d.Status != HeritageDatasetStatus.Draft || d.OwnerUserId == userId);

        if (!string.IsNullOrWhiteSpace(query.Category)
            && Enum.TryParse<HeritageDatasetCategory>(query.Category, true, out var category))
        {
            datasets = datasets.Where(d => d.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<HeritageDatasetStatus>(query.Status, true, out var status))
        {
            datasets = datasets.Where(d => d.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.AccessLevel)
            && Enum.TryParse<HeritageDatasetAccessLevel>(query.AccessLevel, true, out var accessLevel))
        {
            datasets = datasets.Where(d => d.AccessLevel == accessLevel);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim().ToLower();
            datasets = datasets.Where(d => d.Tags != null && d.Tags.ToLower().Contains(tag));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            datasets = datasets.Where(d =>
                d.Name.ToLower().Contains(term) || d.Description.ToLower().Contains(term));
        }

        datasets = datasets.OrderByDescending(d => d.UpdatedAt);

        var totalCount = await datasets.CountAsync(cancellationToken);
        var items = await datasets
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(HeritageDataset dataset, CancellationToken cancellationToken)
        => await _context.HeritageDatasets.AddAsync(dataset, cancellationToken);

    public void Remove(HeritageDataset dataset)
        => _context.HeritageDatasets.Remove(dataset);

    // ---- Versions --------------------------------------------------------

    public Task<HeritageDatasetVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken)
        => _context.HeritageDatasetVersions
            .Include(v => v.CreatedBy)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

    public Task<List<HeritageDatasetVersion>> GetVersionsAsync(Guid datasetId, CancellationToken cancellationToken)
        => _context.HeritageDatasetVersions
            .Include(v => v.CreatedBy)
            .Where(v => v.HeritageDatasetId == datasetId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

    public async Task<int> GetMaxVersionNumberAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var any = await _context.HeritageDatasetVersions
            .Where(v => v.HeritageDatasetId == datasetId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken);

        return any ?? 0;
    }

    public async Task AddVersionAsync(HeritageDatasetVersion version, CancellationToken cancellationToken)
        => await _context.HeritageDatasetVersions.AddAsync(version, cancellationToken);

    // ---- Access grants -------------------------------------------------

    public Task<HeritageDatasetAccessGrant?> GetGrantAsync(Guid datasetId, Guid userId, CancellationToken cancellationToken)
        => _context.HeritageDatasetAccessGrants
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.HeritageDatasetId == datasetId && g.UserId == userId, cancellationToken);

    public Task<HeritageDatasetAccessGrant?> GetGrantByIdAsync(Guid grantId, CancellationToken cancellationToken)
        => _context.HeritageDatasetAccessGrants
            .Include(g => g.User)
            .FirstOrDefaultAsync(g => g.Id == grantId, cancellationToken);

    public Task<List<HeritageDatasetAccessGrant>> GetGrantsAsync(Guid datasetId, CancellationToken cancellationToken)
        => _context.HeritageDatasetAccessGrants
            .Include(g => g.User)
            .Where(g => g.HeritageDatasetId == datasetId)
            .OrderBy(g => g.GrantedAt)
            .ToListAsync(cancellationToken);

    public async Task AddGrantAsync(HeritageDatasetAccessGrant grant, CancellationToken cancellationToken)
        => await _context.HeritageDatasetAccessGrants.AddAsync(grant, cancellationToken);

    public void RemoveGrant(HeritageDatasetAccessGrant grant)
        => _context.HeritageDatasetAccessGrants.Remove(grant);

    // ---- Exports --------------------------------------------------------

    public Task<HeritageDatasetExport?> GetExportByIdAsync(Guid exportId, CancellationToken cancellationToken)
        => _context.HeritageDatasetExports
            .Include(e => e.RequestedBy)
            .Include(e => e.Dataset)
            .Include(e => e.Version)
            .FirstOrDefaultAsync(e => e.Id == exportId, cancellationToken);

    public async Task<(List<HeritageDatasetExport> Items, int TotalCount)> GetExportsForDatasetAsync(
        Guid datasetId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
    {
        var exports = _context.HeritageDatasetExports
            .Include(e => e.RequestedBy)
            .Include(e => e.Dataset)
            .Include(e => e.Version)
            .Where(e => e.HeritageDatasetId == datasetId);

        exports = ApplyExportFilters(exports, query);
        return await PageExportsAsync(exports, query, cancellationToken);
    }

    public async Task<(List<HeritageDatasetExport> Items, int TotalCount)> GetExportsForUserAsync(
        Guid userId, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
    {
        var exports = _context.HeritageDatasetExports
            .Include(e => e.RequestedBy)
            .Include(e => e.Dataset)
            .Include(e => e.Version)
            .Where(e => e.RequestedByUserId == userId);

        exports = ApplyExportFilters(exports, query);
        return await PageExportsAsync(exports, query, cancellationToken);
    }

    private static IQueryable<HeritageDatasetExport> ApplyExportFilters(
        IQueryable<HeritageDatasetExport> exports, HeritageDatasetExportQueryParameters query)
    {
        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<HeritageDatasetExportStatus>(query.Status, true, out var status))
        {
            exports = exports.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Format)
            && Enum.TryParse<HeritageDatasetFileFormat>(query.Format, true, out var format))
        {
            exports = exports.Where(e => e.Format == format);
        }

        return exports;
    }

    private static async Task<(List<HeritageDatasetExport> Items, int TotalCount)> PageExportsAsync(
        IQueryable<HeritageDatasetExport> exports, HeritageDatasetExportQueryParameters query, CancellationToken cancellationToken)
    {
        exports = exports.OrderByDescending(e => e.CreatedAt);
        var totalCount = await exports.CountAsync(cancellationToken);
        var items = await exports
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<HeritageDatasetExportAnalyticsDto> GetExportAnalyticsAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var exports = _context.HeritageDatasetExports.Where(e => e.HeritageDatasetId == datasetId);

        var totals = await exports
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Completed = g.Count(e => e.Status == HeritageDatasetExportStatus.Completed),
                Rows = g.Sum(e => (long)e.RowCount),
                Last = (DateTime?)g.Max(e => e.CreatedAt),
            })
            .FirstOrDefaultAsync(cancellationToken);

        var byFormat = await exports
            .GroupBy(e => e.Format)
            .Select(g => new HeritageCountBucketDto
            {
                Key = g.Key.ToString(),
                Label = g.Key.ToString(),
                Count = g.Count(),
            })
            .ToListAsync(cancellationToken);

        var byMonthRaw = await exports
            .GroupBy(e => new { e.CreatedAt.Year, e.CreatedAt.Month })
            .Select(g => new { g.Key.Year, g.Key.Month, Count = g.Count() })
            .OrderByDescending(x => x.Year).ThenByDescending(x => x.Month)
            .Take(12)
            .ToListAsync(cancellationToken);

        var byMonth = byMonthRaw
            .Select(x => new HeritageCountBucketDto
            {
                Key = $"{x.Year:D4}-{x.Month:D2}",
                Label = $"{x.Year:D4}-{x.Month:D2}",
                Count = x.Count,
            })
            .ToList();

        var topExporters = await exports
            .GroupBy(e => new { e.RequestedByUserId, e.RequestedBy.FullName })
            .Select(g => new HeritageCountBucketDto
            {
                Key = g.Key.RequestedByUserId.ToString(),
                Label = g.Key.FullName,
                Count = g.Count(),
            })
            .OrderByDescending(x => x.Count)
            .Take(5)
            .ToListAsync(cancellationToken);

        return new HeritageDatasetExportAnalyticsDto
        {
            HeritageDatasetId = datasetId,
            TotalExports = totals?.Total ?? 0,
            CompletedExports = totals?.Completed ?? 0,
            TotalRowsExported = totals?.Rows ?? 0,
            LastExportedAt = totals?.Last,
            ByFormat = byFormat,
            ByMonth = byMonth,
            TopExporters = topExporters,
        };
    }

    public async Task AddExportAsync(HeritageDatasetExport export, CancellationToken cancellationToken)
        => await _context.HeritageDatasetExports.AddAsync(export, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
