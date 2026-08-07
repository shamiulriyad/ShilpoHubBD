using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Community;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IDiscussionService
{
    Task<PagedResult<DiscussionThreadListItemDto>> GetAllAsync(DiscussionQueryParameters query, CancellationToken cancellationToken);
    Task<DiscussionThreadDto> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<DiscussionThreadDto> CreateAsync(Guid userId, CreateDiscussionThreadRequest request, CancellationToken cancellationToken);
    Task<DiscussionThreadDto> ReplyAsync(Guid threadId, Guid userId, CreateDiscussionReplyRequest request, CancellationToken cancellationToken);
    Task DeleteThreadAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task DeleteReplyAsync(Guid threadId, Guid replyId, Guid userId, bool isAdmin, CancellationToken cancellationToken);
}
