using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.ProducerBusiness;

namespace ShilpoHubBD.Data.Repositories;

public class ProducerMonthlyReportRepository : IProducerMonthlyReportRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProducerMonthlyReportRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<bool> ExistsAsync(Guid producerId, int year, int month, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReports
            .AnyAsync(r => r.ProducerId == producerId && r.Year == year && r.Month == month, cancellationToken);

    public Task<ProducerMonthlyReport?> GetAsync(Guid producerId, int year, int month, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReports
            .FirstOrDefaultAsync(r => r.ProducerId == producerId && r.Year == year && r.Month == month, cancellationToken);

    public Task<List<ProducerMonthlyReport>> GetAllForMonthAsync(int year, int month, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReports
            .Where(r => r.Year == year && r.Month == month)
            .ToListAsync(cancellationToken);

    public Task<List<Guid>> GetProducerIdsAsync(CancellationToken cancellationToken)
        => _context.Users
            .Where(u => u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ProducerMonthlyReport report, CancellationToken cancellationToken)
        => await _context.ProducerMonthlyReports.AddAsync(report, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);

    private IQueryable<ProducerMonthlyReport> WithDetails()
        => _context.ProducerMonthlyReports
            .Include(r => r.Producer)
            .Include(r => r.Category)
            .Include(r => r.District)
            .AsSplitQuery();

    public Task<ProducerMonthlyReport?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<ProducerMonthlyReport?> GetForProducerWithDetailsAsync(Guid producerId, int year, int month, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.ProducerId == producerId && r.Year == year && r.Month == month, cancellationToken);

    public Task<ProducerMonthlyReport?> GetLatestForProducerAsync(Guid producerId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(r => r.ProducerId == producerId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .FirstOrDefaultAsync(cancellationToken);

    private IQueryable<ProducerMonthlyReport> ApplyFilters(IQueryable<ProducerMonthlyReport> reports, ProducerMonthlyReportQueryParameters query)
    {
        if (query.Year.HasValue)
        {
            reports = reports.Where(r => r.Year == query.Year.Value);
        }

        if (query.Month.HasValue)
        {
            reports = reports.Where(r => r.Month == query.Month.Value);
        }

        if (query.DistrictId.HasValue)
        {
            reports = reports.Where(r => r.DistrictId == query.DistrictId.Value);
        }

        if (query.CategoryId.HasValue)
        {
            reports = reports.Where(r => r.CategoryId == query.CategoryId.Value);
        }

        if (query.ProducerId.HasValue)
        {
            reports = reports.Where(r => r.ProducerId == query.ProducerId.Value);
        }

        if (query.SharedWithUserId.HasValue)
        {
            reports = reports.Where(r => _context.ProducerMonthlyReportShares
                .Any(s => s.ReportId == r.Id && s.SharedWithUserId == query.SharedWithUserId.Value));
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            reports = reports.Where(r =>
                r.Producer.FullName.ToLower().Contains(term) || r.Producer.Email.ToLower().Contains(term));
        }

        return reports;
    }

    public async Task<(List<ProducerMonthlyReport> Items, int TotalCount)> GetPagedAsync(
        ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
    {
        var reports = ApplyFilters(WithDetails(), query)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month).ThenBy(r => r.OverallSalesRank);

        var totalCount = await reports.CountAsync(cancellationToken);
        var items = await reports
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>Every report matching the filters (Page/PageSize ignored) — for dashboard aggregates over the whole filtered set, not just one page.</summary>
    public Task<List<ProducerMonthlyReport>> GetAllMatchingAsync(ProducerMonthlyReportQueryParameters query, CancellationToken cancellationToken)
        => ApplyFilters(WithDetails(), query).ToListAsync(cancellationToken);

    public Task<bool> ReportExistsAsync(Guid reportId, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReports.AnyAsync(r => r.Id == reportId, cancellationToken);

    public Task<List<Guid>> FilterByRoleAsync(IEnumerable<Guid> userIds, string roleName, CancellationToken cancellationToken)
        => _context.Users
            .Where(u => userIds.Contains(u.Id) && u.UserRoles.Any(ur => ur.Role.Name == roleName))
            .Select(u => u.Id)
            .ToListAsync(cancellationToken);

    public Task<bool> IsSharedWithAsync(Guid reportId, Guid userId, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReportShares
            .AnyAsync(s => s.ReportId == reportId && s.SharedWithUserId == userId, cancellationToken);

    public Task<List<Guid>> GetExistingShareUserIdsAsync(Guid reportId, IEnumerable<Guid> sharedWithUserIds, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReportShares
            .Where(s => s.ReportId == reportId && sharedWithUserIds.Contains(s.SharedWithUserId))
            .Select(s => s.SharedWithUserId)
            .ToListAsync(cancellationToken);

    public async Task AddShareAsync(ProducerMonthlyReportShare share, CancellationToken cancellationToken)
        => await _context.ProducerMonthlyReportShares.AddAsync(share, cancellationToken);

    public Task<List<ProducerMonthlyReportShare>> GetSharesForReportAsync(Guid reportId, CancellationToken cancellationToken)
        => _context.ProducerMonthlyReportShares
            .Include(s => s.SharedWithUser)
            .Include(s => s.SharedByUser)
            .Where(s => s.ReportId == reportId)
            .OrderByDescending(s => s.SharedAt)
            .ToListAsync(cancellationToken);
}
