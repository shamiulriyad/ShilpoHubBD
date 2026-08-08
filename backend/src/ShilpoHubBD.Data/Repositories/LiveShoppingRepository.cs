using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Data.Repositories;

public class LiveShoppingRepository : ILiveShoppingRepository
{
    private readonly ShilpoHubDbContext _context;

    public LiveShoppingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<LiveEvent> WithDetails()
        => _context.LiveEvents
            .Include(e => e.Producer)
            .Include(e => e.Product).ThenInclude(p => p.Images)
            .Include(e => e.Comments).ThenInclude(c => c.User)
            .Include(e => e.Reactions)
            .Include(e => e.Purchases)
            .AsSplitQuery();

    public async Task<(List<LiveEvent> Items, int TotalCount)> GetPagedAsync(LiveEventStatus? status, int page, int pageSize, CancellationToken cancellationToken)
    {
        var events = WithDetails().AsQueryable();

        if (status is not null)
        {
            events = events.Where(e => e.Status == status);
        }

        events = events.OrderByDescending(e => e.ScheduledStartAt);

        var totalCount = await events.CountAsync(cancellationToken);
        var items = await events
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<LiveEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task AddAsync(LiveEvent liveEvent, CancellationToken cancellationToken)
        => await _context.LiveEvents.AddAsync(liveEvent, cancellationToken);

    public async Task AddCommentAsync(LiveEventComment comment, CancellationToken cancellationToken)
        => await _context.LiveEventComments.AddAsync(comment, cancellationToken);

    public async Task AddReactionAsync(LiveEventReaction reaction, CancellationToken cancellationToken)
        => await _context.LiveEventReactions.AddAsync(reaction, cancellationToken);

    public async Task AddPurchaseAsync(LiveEventPurchase purchase, CancellationToken cancellationToken)
        => await _context.LiveEventPurchases.AddAsync(purchase, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
