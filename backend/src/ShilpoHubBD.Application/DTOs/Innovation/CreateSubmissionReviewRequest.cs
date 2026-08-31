namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreateSubmissionReviewRequest
{
    public string Decision { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Comments { get; set; } = string.Empty;
}
