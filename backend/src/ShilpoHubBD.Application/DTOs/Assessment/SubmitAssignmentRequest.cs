namespace ShilpoHubBD.Application.DTOs.Assessment;

public class SubmitAssignmentRequest
{
    public string SubmissionText { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}
