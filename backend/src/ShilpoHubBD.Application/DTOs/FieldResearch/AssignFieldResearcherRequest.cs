namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class AssignFieldResearcherRequest
{
    public Guid FieldResearcherUserId { get; set; }
    public string? Role { get; set; }
    public string? AreaNote { get; set; }
}
