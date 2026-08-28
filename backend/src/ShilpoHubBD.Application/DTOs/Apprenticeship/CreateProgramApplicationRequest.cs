namespace ShilpoHubBD.Application.DTOs.Apprenticeship;

public class CreateProgramApplicationRequest
{
    public Guid ProgramId { get; set; }
    public string Message { get; set; } = string.Empty;
}
