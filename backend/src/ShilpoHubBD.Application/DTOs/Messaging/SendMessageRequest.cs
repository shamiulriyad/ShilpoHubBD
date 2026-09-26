namespace ShilpoHubBD.Application.DTOs.Messaging;

public class SendMessageRequest
{
    public string Body { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}
