using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Admin;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data.Repositories;

public class AdminUserRepository : IAdminUserRepository
{
    private readonly ShilpoHubDbContext _context;

    public AdminUserRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<User> Items, int TotalCount)> GetPagedAsync(
        AdminUserQueryParameters query, CancellationToken cancellationToken)
    {
        var users = _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            users = users.Where(u =>
                u.FullName.ToLower().Contains(term) || u.Email.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(query.Role))
        {
            users = users.Where(u => u.UserRoles.Any(ur => ur.Role.Name == query.Role));
        }

        if (query.IsActive.HasValue)
        {
            users = users.Where(u => u.IsActive == query.IsActive.Value);
        }

        users = users.OrderByDescending(u => u.CreatedAt);

        var totalCount = await users.CountAsync(cancellationToken);
        var items = await users
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken)
        => _context.Users
            .Include(u => u.UserRoles).ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
