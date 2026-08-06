using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly ShilpoHubDbContext _context;

    public RoleRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken)
        => _context.Roles.FirstOrDefaultAsync(r => r.Name == name, cancellationToken);

    public Task<List<Role>> GetByNamesAsync(IEnumerable<string> names, CancellationToken cancellationToken)
        => _context.Roles.Where(r => names.Contains(r.Name)).ToListAsync(cancellationToken);
}
