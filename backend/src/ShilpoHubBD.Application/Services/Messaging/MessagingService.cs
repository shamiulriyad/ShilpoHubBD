using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Messaging;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Messaging;

namespace ShilpoHubBD.Application.Services.Messaging;

public class MessagingService : IMessagingService
{
    private readonly IMessagingRepository _messagingRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMessageNotifier _messageNotifier;

    public MessagingService(
        IMessagingRepository messagingRepository,
        IUserRepository userRepository,
        IMessageNotifier messageNotifier)
    {
        _messagingRepository = messagingRepository;
        _userRepository = userRepository;
        _messageNotifier = messageNotifier;
    }

    public async Task<PagedResult<ConversationListItemDto>> GetConversationsAsync(Guid userId, ConversationQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _messagingRepository.GetPagedForUserAsync(userId, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<ConversationListItemDto>
        {
            Items = items.Select(c => ToListItemDto(c, userId)).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ConversationDto> GetConversationByIdAsync(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        var conversation = await _messagingRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(conversation, userId);

        return ToDto(conversation);
    }

    public async Task<ConversationDto> StartConversationAsync(Guid userId, StartConversationRequest request, CancellationToken cancellationToken)
    {
        if (request.RecipientId == userId)
        {
            throw new ConflictException("You cannot message yourself.");
        }

        if (await _userRepository.GetByIdAsync(request.RecipientId, cancellationToken) is null)
        {
            throw new NotFoundException("Recipient not found.");
        }

        var conversation = await _messagingRepository.GetBetweenUsersAsync(userId, request.RecipientId, cancellationToken);
        var now = DateTime.UtcNow;

        if (conversation is null)
        {
            conversation = new Conversation
            {
                Id = Guid.NewGuid(),
                CreatedAt = now,
                UpdatedAt = now,
                Participants = new List<ConversationParticipant>
                {
                    new() { Id = Guid.NewGuid(), UserId = userId },
                    new() { Id = Guid.NewGuid(), UserId = request.RecipientId },
                },
            };

            await _messagingRepository.AddAsync(conversation, cancellationToken);
        }
        else
        {
            conversation.UpdatedAt = now;
        }

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversation.Id,
            SenderId = userId,
            Body = request.Body.Trim(),
            CreatedAt = now,
        };

        await _messagingRepository.AddMessageAsync(message, cancellationToken);
        await _messagingRepository.SaveChangesAsync(cancellationToken);

        var created = await _messagingRepository.GetByIdAsync(conversation.Id, cancellationToken);
        var dto = ToDto(created!);

        await _messageNotifier.NotifyMessageAsync(request.RecipientId, dto.Messages.Last(), cancellationToken);

        return dto;
    }

    public async Task<MessageDto> SendMessageAsync(Guid conversationId, Guid userId, SendMessageRequest request, CancellationToken cancellationToken)
    {
        var conversation = await _messagingRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation not found.");

        EnsureParticipant(conversation, userId);

        var message = new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            SenderId = userId,
            Body = request.Body.Trim(),
            CreatedAt = DateTime.UtcNow,
        };

        await _messagingRepository.AddMessageAsync(message, cancellationToken);
        conversation.UpdatedAt = message.CreatedAt;
        await _messagingRepository.SaveChangesAsync(cancellationToken);

        var senderName = conversation.Participants.First(p => p.UserId == userId).User.FullName;
        var dto = ToMessageDto(message, senderName, isRead: false);

        foreach (var recipient in conversation.Participants.Where(p => p.UserId != userId))
        {
            await _messageNotifier.NotifyMessageAsync(recipient.UserId, dto, cancellationToken);
        }

        return dto;
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken cancellationToken)
    {
        var conversation = await _messagingRepository.GetByIdAsync(conversationId, cancellationToken)
            ?? throw new NotFoundException("Conversation not found.");

        var participant = conversation.Participants.FirstOrDefault(p => p.UserId == userId)
            ?? throw new UnauthorizedAccessException("You are not a participant in this conversation.");

        participant.LastReadAt = DateTime.UtcNow;
        await _messagingRepository.SaveChangesAsync(cancellationToken);
    }

    private static void EnsureParticipant(Conversation conversation, Guid userId)
    {
        if (conversation.Participants.All(p => p.UserId != userId))
        {
            throw new UnauthorizedAccessException("You are not a participant in this conversation.");
        }
    }

    private static ConversationListItemDto ToListItemDto(Conversation conversation, Guid userId)
    {
        var me = conversation.Participants.First(p => p.UserId == userId);
        var other = conversation.Participants.First(p => p.UserId != userId);
        var lastMessage = conversation.Messages.OrderByDescending(m => m.CreatedAt).FirstOrDefault();

        return new ConversationListItemDto
        {
            Id = conversation.Id,
            OtherUserId = other.UserId,
            OtherUserName = other.User.FullName,
            LastMessageBody = lastMessage?.Body,
            LastMessageAt = lastMessage?.CreatedAt,
            UnreadCount = conversation.Messages.Count(m => m.SenderId != userId && (me.LastReadAt == null || m.CreatedAt > me.LastReadAt)),
            UpdatedAt = conversation.UpdatedAt,
        };
    }

    private static ConversationDto ToDto(Conversation conversation) => new()
    {
        Id = conversation.Id,
        CreatedAt = conversation.CreatedAt,
        UpdatedAt = conversation.UpdatedAt,
        Participants = conversation.Participants
            .Select(p => new ConversationParticipantDto { UserId = p.UserId, FullName = p.User.FullName })
            .ToList(),
        Messages = conversation.Messages
            .OrderBy(m => m.CreatedAt)
            .Select(m => ToMessageDto(m, m.Sender.FullName, IsReadByOthers(conversation, m)))
            .ToList(),
    };

    private static bool IsReadByOthers(Conversation conversation, Message message)
        => conversation.Participants
            .Where(p => p.UserId != message.SenderId)
            .All(p => p.LastReadAt is not null && p.LastReadAt >= message.CreatedAt);

    private static MessageDto ToMessageDto(Message message, string senderName, bool isRead) => new()
    {
        Id = message.Id,
        ConversationId = message.ConversationId,
        SenderId = message.SenderId,
        SenderName = senderName,
        Body = message.Body,
        CreatedAt = message.CreatedAt,
        IsRead = isRead,
    };
}
