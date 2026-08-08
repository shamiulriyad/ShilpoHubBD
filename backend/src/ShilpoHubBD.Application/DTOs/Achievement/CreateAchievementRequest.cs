namespace ShilpoHubBD.Application.DTOs.Achievement;

public class CreateAchievementRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int RequiredXp { get; set; }
    public int XpReward { get; set; }
}
