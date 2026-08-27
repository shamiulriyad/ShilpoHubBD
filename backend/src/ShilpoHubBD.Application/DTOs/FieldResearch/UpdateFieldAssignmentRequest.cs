namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class UpdateFieldAssignmentRequest
{
    public string Role { get; set; } = string.Empty;
    public string? AreaNote { get; set; }
    public bool IsActive { get; set; }
}
