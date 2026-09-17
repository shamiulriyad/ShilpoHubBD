using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Security;

namespace ShilpoHubBD.Data.Repositories;

public class ApiKeyRepository : IApiKeyRepository
{
    private readonly ShilpoHubDbContext _context;

    public ApiKeyRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<ApiKey> WithDetails()
        => _context.ApiKeys.Include(k => k.CreatedBy);

    public async Task AddAsync(ApiKey key, CancellationToken cancellationToken)
        => await _context.ApiKeys.AddAsync(key, cancellationToken);

    public Task<ApiKey?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(k => k.Id == id, cancellationToken);

    public async Task<(List<ApiKey> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, CancellationToken cancellationToken)
    {
        var keys = WithDetails().OrderByDescending(k => k.CreatedAt);

        var totalCount = await keys.CountAsync(cancellationToken);
        var items = await keys.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
