using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Data.Repositories;

public class DiscussionRepository : IDiscussionRepository
{
    private readonly ShilpoHubDbContext _context;

    public DiscussionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<DiscussionThread> WithDetails()
        => _context.DiscussionThreads
            .Include(t => t.User)
            .Include(t => t.Replies).ThenInclude(r => r.User)
            .AsSplitQuery();

    public async Task<(List<DiscussionThread> Items, int TotalCount)> GetPagedAsync(string? category, int page, int pageSize, CancellationToken cancellationToken)
    {
        var threads = WithDetails().AsQueryable();

        if (!string.IsNullOrWhiteSpace(category))
        {
            threads = threads.Where(t => t.Category == category);
        }

        threads = threads.OrderByDescending(t => t.CreatedAt);

        var totalCount = await threads.CountAsync(cancellationToken);
        var items = await threads
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<DiscussionThread?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task AddAsync(DiscussionThread thread, CancellationToken cancellationToken)
        => await _context.DiscussionThreads.AddAsync(thread, cancellationToken);

    public async Task AddReplyAsync(DiscussionReply reply, CancellationToken cancellationToken)
        => await _context.DiscussionReplies.AddAsync(reply, cancellationToken);

    public void Remove(DiscussionThread thread)
        => _context.DiscussionThreads.Remove(thread);

    public void RemoveReply(DiscussionReply reply)
        => _context.DiscussionReplies.Remove(reply);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
