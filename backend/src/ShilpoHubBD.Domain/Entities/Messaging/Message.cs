using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Messaging;

public class Message
{
    public Guid Id { get; set; }

    public Guid ConversationId { get; set; }
    public Conversation Conversation { get; set; } = null!;

    public Guid SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public string Body { get; set; } = string.Empty;

    /// <summary>Optional picture sent in the chat (uploaded through api/media/chat-images).</summary>
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
