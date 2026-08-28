namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ProgramApplicationDto
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string ProgramTitle { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
    public string? ResponseMessage { get; set; }
}
