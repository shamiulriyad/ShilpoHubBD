namespace ShilpoHubBD.Application.DTOs.Community;

public class AnswerDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public bool IsProducerAnswer { get; set; }
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
