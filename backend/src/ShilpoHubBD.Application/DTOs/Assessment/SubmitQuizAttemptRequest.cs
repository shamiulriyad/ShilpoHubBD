namespace ShilpoHubBD.Application.DTOs.Assessment;

public class SubmitQuizAttemptRequest
{
    public List<QuizAnswerSubmission> Answers { get; set; } = new();
}
