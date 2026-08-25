namespace ShilpoHubBD.Application.DTOs.Roadmap;

public class CreateRoadmapRequest
{
    public string Goal { get; set; } = string.Empty;
    public Guid? TargetHeritageSkillId { get; set; }
}
