namespace ShilpoHubBD.Application.DTOs.Assessment;

public class ExamAnswerSubmission
{
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
    public string? EssayAnswerText { get; set; }
}
