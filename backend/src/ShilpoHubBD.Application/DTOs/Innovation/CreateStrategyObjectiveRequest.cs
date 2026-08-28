namespace ShilpoHubBD.Application.DTOs.Innovation;

public class CreateStrategyObjectiveRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int OrderIndex { get; set; }
}
