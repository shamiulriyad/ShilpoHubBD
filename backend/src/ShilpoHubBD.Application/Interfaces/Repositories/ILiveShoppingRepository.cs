using ShilpoHubBD.Domain.Entities.LiveShopping;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface ILiveShoppingRepository
{
    Task<(List<LiveEvent> Items, int TotalCount)> GetPagedAsync(LiveEventStatus? status, int page, int pageSize, CancellationToken cancellationToken);
    Task<LiveEvent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(LiveEvent liveEvent, CancellationToken cancellationToken);
    Task AddCommentAsync(LiveEventComment comment, CancellationToken cancellationToken);
    Task AddReactionAsync(LiveEventReaction reaction, CancellationToken cancellationToken);
    Task AddPurchaseAsync(LiveEventPurchase purchase, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
