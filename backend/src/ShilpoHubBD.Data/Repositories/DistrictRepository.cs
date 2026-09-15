using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class DistrictRepository : IDistrictRepository
{
    private readonly ShilpoHubDbContext _context;

    public DistrictRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<District>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken)
    {
        var query = includeInactive ? _context.Districts.AsQueryable() : _context.Districts.Where(d => d.IsActive);
        return query.OrderBy(d => d.DisplayOrder).ToListAsync(cancellationToken);
    }

    public Task<District?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Districts.FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
