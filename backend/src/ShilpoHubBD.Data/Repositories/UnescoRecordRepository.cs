using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;

namespace ShilpoHubBD.Data.Repositories;

public class UnescoRecordRepository : IUnescoRecordRepository
{
    private readonly ShilpoHubDbContext _context;

    public UnescoRecordRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<UnescoRecord>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = _context.UnescoRecords.Include(r => r.District).AsQueryable();
        if (!includeInactive)
        {
            query = query.Where(r => r.IsActive);
        }

        return query.OrderBy(r => r.DisplayOrder).ThenBy(r => r.Title).ToListAsync(cancellationToken);
    }

    public Task<UnescoRecord?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.UnescoRecords.Include(r => r.District).FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<bool> DistrictExistsAsync(Guid districtId, CancellationToken cancellationToken)
        => _context.Districts.AnyAsync(d => d.Id == districtId, cancellationToken);

    public async Task AddAsync(UnescoRecord record, CancellationToken cancellationToken)
        => await _context.UnescoRecords.AddAsync(record, cancellationToken);

    public void Remove(UnescoRecord record) => _context.UnescoRecords.Remove(record);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
