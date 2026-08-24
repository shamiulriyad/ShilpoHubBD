namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class LiveClassParticipantDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime RegisteredAt { get; set; }
}
