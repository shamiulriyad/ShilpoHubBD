namespace ShilpoHubBD.Application.DTOs.Assessment;

public class UpdateQuizQuestionRequest
{
    public string Body { get; set; } = string.Empty;
    public int Points { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public List<CreateQuizQuestionOptionRequest> Options { get; set; } = new();
}
