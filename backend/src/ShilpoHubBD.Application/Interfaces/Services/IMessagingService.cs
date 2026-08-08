using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Messaging;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IMessagingService
{
    Task<PagedResult<ConversationListItemDto>> GetConversationsAsync(Guid userId, ConversationQueryParameters query, CancellationToken cancellationToken);
    Task<ConversationDto> GetConversationByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken);
    Task<ConversationDto> StartConversationAsync(Guid userId, StartConversationRequest request, CancellationToken cancellationToken);
    Task<MessageDto> SendMessageAsync(Guid conversationId, Guid userId, SendMessageRequest request, CancellationToken cancellationToken);
    Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken);
}
