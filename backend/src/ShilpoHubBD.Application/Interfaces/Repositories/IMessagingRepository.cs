using ShilpoHubBD.Domain.Entities.Messaging;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IMessagingRepository
{
    Task<(List<Conversation> Items, int TotalCount)> GetPagedForUserAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken);
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Conversation?> GetBetweenUsersAsync(Guid userId1, Guid userId2, CancellationToken cancellationToken);
    Task AddAsync(Conversation conversation, CancellationToken cancellationToken);
    Task AddMessageAsync(Message message, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
