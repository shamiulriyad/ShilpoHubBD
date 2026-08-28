namespace ShilpoHubBD.Application.DTOs.Employment;

public class JobApplicationListItemDto
{
    public Guid Id { get; set; }
    public Guid JobListingId { get; set; }
    public string JobTitle { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}
