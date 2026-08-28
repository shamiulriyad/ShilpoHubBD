namespace ShilpoHubBD.Application.DTOs.Assessment;

public class QuizAnswerSubmission
{
    public Guid QuestionId { get; set; }
    public Guid? SelectedOptionId { get; set; }
}
