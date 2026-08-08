namespace ShilpoHubBD.Application.DTOs.Achievement;

public class AchievementDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public int RequiredXp { get; set; }
    public int XpReward { get; set; }
    public DateTime CreatedAt { get; set; }
}
