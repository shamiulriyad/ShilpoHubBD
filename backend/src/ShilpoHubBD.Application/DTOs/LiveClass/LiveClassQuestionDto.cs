namespace ShilpoHubBD.Application.DTOs.LiveClass;

public class LiveClassQuestionDto
{
    public Guid Id { get; set; }
    public Guid LiveClassId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsAnswered { get; set; }
    public string? AnswerBody { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AnsweredAt { get; set; }
}
