namespace ShilpoHubBD.Application.DTOs.Innovation;

public class UpdateStrategyObjectiveRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
    public bool IsAchieved { get; set; }
}
