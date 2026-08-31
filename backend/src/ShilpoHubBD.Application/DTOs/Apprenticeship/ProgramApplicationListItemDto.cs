namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class ProgramApplicationListItemDto
{
    public Guid Id { get; set; }
    public Guid ProgramId { get; set; }
    public string ProgramTitle { get; set; } = string.Empty;
    public Guid ApplicantUserId { get; set; }
    public string ApplicantName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime AppliedAt { get; set; }
}
