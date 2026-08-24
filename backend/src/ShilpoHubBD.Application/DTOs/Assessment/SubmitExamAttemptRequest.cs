namespace ShilpoHubBD.Application.DTOs.Assessment;

public class SubmitExamAttemptRequest
{
    public List<ExamAnswerSubmission> Answers { get; set; } = new();
}
