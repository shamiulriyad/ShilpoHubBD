namespace ShilpoHubBD.Application.DTOs.Messaging;

public class StartConversationRequest
{
    public Guid RecipientId { get; set; }
    public string Body { get; set; } = string.Empty;
}
