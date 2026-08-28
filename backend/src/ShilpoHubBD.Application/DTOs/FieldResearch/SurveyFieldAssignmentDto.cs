namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyFieldAssignmentDto
{
    public Guid Id { get; set; }
    public Guid SurveyId { get; set; }
    public Guid FieldResearcherUserId { get; set; }
    public string FieldResearcherName { get; set; } = string.Empty;
    public string FieldResearcherEmail { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? AreaNote { get; set; }
    public bool IsActive { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTime AssignedAt { get; set; }
}
