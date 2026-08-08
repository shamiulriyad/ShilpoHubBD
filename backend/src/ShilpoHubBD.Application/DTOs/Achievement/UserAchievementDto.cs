namespace ShilpoHubBD.Application.DTOs.Achievement;

public class UserAchievementDto
{
    public Guid Id { get; set; }
    public Guid AchievementId { get; set; }
    public string AchievementName { get; set; } = string.Empty;
    public string? IconUrl { get; set; }
    public DateTime UnlockedAt { get; set; }
}
