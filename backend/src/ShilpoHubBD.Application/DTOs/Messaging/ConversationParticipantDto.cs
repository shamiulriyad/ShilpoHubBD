namespace ShilpoHubBD.Application.DTOs.Messaging;

public class ConversationParticipantDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
}
