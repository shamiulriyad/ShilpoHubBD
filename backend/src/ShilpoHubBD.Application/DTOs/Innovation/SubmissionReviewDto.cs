namespace ShilpoHubBD.Application.DTOs.Innovation;

public class SubmissionReviewDto
{
    public Guid Id { get; set; }
    public Guid ReviewerUserId { get; set; }
    public string ReviewerName { get; set; } = string.Empty;
    public string Decision { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string Comments { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
