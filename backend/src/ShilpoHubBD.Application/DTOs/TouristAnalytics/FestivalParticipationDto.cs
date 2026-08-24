namespace ShilpoHubBD.Application.DTOs.TouristAnalytics;

public class FestivalParticipationDto
{
    public int FestivalBadgeCount { get; set; }
    public List<string> FestivalNames { get; set; } = new();
}
