using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ShilpoHubDbContext _context;

    public UserRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken)
        => _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken)
        => _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Email == email, cancellationToken);

    public Task<bool> AnyInRoleAsync(string roleName, CancellationToken cancellationToken)
        => _context.UserRoles.AnyAsync(ur => ur.Role.Name == roleName, cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
        => await _context.Users.AddAsync(user, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
