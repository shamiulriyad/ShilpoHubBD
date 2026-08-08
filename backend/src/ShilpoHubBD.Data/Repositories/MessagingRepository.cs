using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Messaging;

namespace ShilpoHubBD.Data.Repositories;

public class MessagingRepository : IMessagingRepository
{
    private readonly ShilpoHubDbContext _context;

    public MessagingRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<Conversation> WithDetails()
        => _context.Conversations
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Messages).ThenInclude(m => m.Sender)
            .AsSplitQuery();

    public async Task<(List<Conversation> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken)
    {
        var conversations = WithDetails()
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.UpdatedAt);

        var totalCount = await conversations.CountAsync(cancellationToken);
        var items = await conversations
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public Task<Conversation?> GetBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c =>
            c.Participants.Count == 2 &&
            c.Participants.Any(p => p.UserId == userId1) &&
            c.Participants.Any(p => p.UserId == userId2),
            cancellationToken);

    public async Task AddAsync(Conversation conversation, CancellationToken cancellationToken)
        => await _context.Conversations.AddAsync(conversation, cancellationToken);

    public async Task AddMessageAsync(Message message, CancellationToken cancellationToken)
        => await _context.Messages.AddAsync(message, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
