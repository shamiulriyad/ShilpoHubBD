namespace ShilpoHubBD.Application.DTOs.Assessment;

public class CreateQuizQuestionOptionRequest
{
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public int DisplayOrder { get; set; }
}
