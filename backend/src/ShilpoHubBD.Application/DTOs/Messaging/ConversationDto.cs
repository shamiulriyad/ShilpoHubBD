namespace ShilpoHubBD.Application.DTOs.Messaging;

public class ConversationDto
{
    public Guid Id { get; set; }
    public List<ConversationParticipantDto> Participants { get; set; } = new();
    public List<MessageDto> Messages { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
