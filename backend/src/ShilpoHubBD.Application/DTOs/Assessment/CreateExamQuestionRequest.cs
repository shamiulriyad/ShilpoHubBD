using ShilpoHubBD.Domain.Entities.Assessment;

namespace ShilpoHubBD.Application.DTOs.Assessment;

public class CreateExamQuestionRequest
{
    public string Body { get; set; } = string.Empty;
    public QuestionType QuestionType { get; set; } = QuestionType.MultipleChoice;
    public int Points { get; set; } = 1;
    public int DisplayOrder { get; set; }
    public List<CreateExamQuestionOptionRequest> Options { get; set; } = new();
}
