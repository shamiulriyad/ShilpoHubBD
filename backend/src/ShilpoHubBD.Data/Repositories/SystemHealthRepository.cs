using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;

namespace ShilpoHubBD.Data.Repositories;

public class SystemHealthRepository : ISystemHealthRepository
{
    private readonly ShilpoHubDbContext _context;

    public SystemHealthRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanConnectAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await _context.Database.CanConnectAsync(cancellationToken);
        }
        catch
        {
            return false;
        }
    }

    public async Task<(int Users, int Orders, int Products)> GetCountsAsync(CancellationToken cancellationToken)
        => (
            await _context.Users.CountAsync(cancellationToken),
            await _context.Orders.CountAsync(cancellationToken),
            await _context.Products.CountAsync(cancellationToken));
}
