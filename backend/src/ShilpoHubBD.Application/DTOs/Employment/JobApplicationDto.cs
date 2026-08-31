namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobApplicationDto
{
    public Guid Id { get; set; }
    public Guid JobListingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public string EmployerName { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string CoverMessage { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
}
