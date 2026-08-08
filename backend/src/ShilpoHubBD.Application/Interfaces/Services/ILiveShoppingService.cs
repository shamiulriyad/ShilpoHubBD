using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.LiveShopping;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface ILiveShoppingService
{
    Task<PagedResult<LiveEventListItemDto>> GetAllAsync(LiveEventQueryParameters query, CancellationToken cancellationToken);
    Task<LiveEventDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<LiveEventDto> CreateAsync(Guid producerId, CreateLiveEventRequest request, CancellationToken cancellationToken);
    Task<LiveEventDto> StartAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
    Task<LiveEventDto> EndAsync(Guid id, Guid producerId, bool isAdmin, CancellationToken cancellationToken);
    Task<LiveEventCommentDto> AddCommentAsync(Guid id, Guid userId, AddLiveCommentRequest request, CancellationToken cancellationToken);
    Task<List<ReactionSummaryDto>> AddReactionAsync(Guid id, Guid userId, AddLiveReactionRequest request, CancellationToken cancellationToken);
    Task<CartItemDto> BuyDuringLiveAsync(Guid id, Guid userId, BuyDuringLiveRequest request, CancellationToken cancellationToken);
}
