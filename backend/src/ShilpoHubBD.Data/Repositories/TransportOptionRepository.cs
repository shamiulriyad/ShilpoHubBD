using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Tourism;

namespace ShilpoHubBD.Data.Repositories;

public class TransportOptionRepository : ITransportOptionRepository
{
    private readonly ShilpoHubDbContext _context;

    public TransportOptionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<TransportOption>> GetForDestinationAsync(string destinationDistrict, string mode, CancellationToken cancellationToken)
    {
        var district = destinationDistrict.ToLower();
        var wantedMode = mode.ToLower();
        return _context.TransportOptions
            .AsNoTracking()
            .Where(t => t.IsActive && t.DestinationDistrict.ToLower() == district && t.Mode.ToLower() == wantedMode)
            .OrderBy(t => t.Operator)
            .ThenBy(t => t.ServiceName)
            .ToListAsync(cancellationToken);
    }
}
