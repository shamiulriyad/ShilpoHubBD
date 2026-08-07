namespace ShilpoHubBD.Application.DTOs.Community;

public class QuestionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid UserId { get; set; }
    public string AskerName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public List<AnswerDto> Answers { get; set; } = new();
}
