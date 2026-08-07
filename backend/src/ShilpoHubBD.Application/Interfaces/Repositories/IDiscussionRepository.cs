using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IDiscussionRepository
{
    Task<(List<DiscussionThread> Items, int TotalCount)> GetPagedAsync(string? category, int page, int pageSize, CancellationToken cancellationToken);
    Task<DiscussionThread?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(DiscussionThread thread, CancellationToken cancellationToken);
    Task AddReplyAsync(DiscussionReply reply, CancellationToken cancellationToken);
    void Remove(DiscussionThread thread);
    void RemoveReply(DiscussionReply reply);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
