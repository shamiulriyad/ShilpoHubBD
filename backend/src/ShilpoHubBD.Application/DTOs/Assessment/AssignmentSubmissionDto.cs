namespace ShilpoHubBD.Application.DTOs.Assessment;

public class AssignmentSubmissionDto
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public string AssignmentTitle { get; set; } = string.Empty;
    public int MaxScore { get; set; }
    public Guid StudentUserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string SubmissionText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public string Status { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? Feedback { get; set; }
    public DateTime SubmittedAt { get; set; }
    public DateTime? GradedAt { get; set; }
}
