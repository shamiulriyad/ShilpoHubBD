using ShilpoHubBD.Application.DTOs.Achievement;

namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class CulturalAchievementsSummaryDto
{
    public int TotalBadges { get; set; }
    public Dictionary<string, int> BadgeCountsByType { get; set; } = new();
    public XpSummaryDto XpSummary { get; set; } = new();
}
