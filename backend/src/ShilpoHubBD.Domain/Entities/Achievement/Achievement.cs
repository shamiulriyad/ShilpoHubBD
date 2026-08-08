namespace ShilpoHubBD.Domain.Entities.Achievement;

public class Achievement
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? IconUrl { get; set; }

    // Total XP the user must have accumulated to unlock this achievement.
    public int RequiredXp { get; set; }

    // Bonus XP granted the moment this achievement unlocks (can be 0).
    public int XpReward { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new List<UserAchievement>();
}
