using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageRiskRepository : IHeritageRiskRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageRiskRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<HeritageRiskRecord> WithDetails()
        => _context.HeritageRiskRecords
            .Include(r => r.District)
            .Include(r => r.Village)
            .Include(r => r.Producer)
            .Include(r => r.CreatedBy);

    public Task<HeritageRiskRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public async Task<(List<HeritageRiskRecord> Items, int TotalCount)> GetPagedAsync(
        HeritageRiskQueryParameters query, CancellationToken cancellationToken)
    {
        var records = WithDetails();

        if (!string.IsNullOrWhiteSpace(query.Category)
            && Enum.TryParse<HeritageRiskCategory>(query.Category, true, out var category))
        {
            records = records.Where(r => r.Category == category);
        }

        if (!string.IsNullOrWhiteSpace(query.Level)
            && Enum.TryParse<HeritageRiskLevel>(query.Level, true, out var level))
        {
            records = records.Where(r => r.Level == level);
        }

        if (query.DistrictId.HasValue)
        {
            records = records.Where(r => r.DistrictId == query.DistrictId.Value);
        }

        if (query.VillageId.HasValue)
        {
            records = records.Where(r => r.VillageId == query.VillageId.Value);
        }

        if (query.AssessmentYear.HasValue)
        {
            records = records.Where(r => r.AssessmentYear == query.AssessmentYear.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            records = records.Where(r =>
                r.Title.ToLower().Contains(term)
                || r.Description.ToLower().Contains(term)
                || (r.CraftName != null && r.CraftName.ToLower().Contains(term)));
        }

        records = records
            .OrderByDescending(r => r.Level)
            .ThenByDescending(r => r.UpdatedAt);

        var totalCount = await records.CountAsync(cancellationToken);
        var items = await records
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(HeritageRiskRecord record, CancellationToken cancellationToken)
        => await _context.HeritageRiskRecords.AddAsync(record, cancellationToken);

    public void Remove(HeritageRiskRecord record)
        => _context.HeritageRiskRecords.Remove(record);

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> VillageExistsAsync(Guid villageId, CancellationToken cancellationToken)
        => _context.Villages.AnyAsync(v => v.Id == villageId, cancellationToken);

    public Task<bool> ProducerExistsAsync(Guid producerId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(
            u => u.Id == producerId && u.UserRoles.Any(ur => ur.Role.Name == RoleNames.Producer), cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
