namespace ShilpoHubBD.Application.DTOs.Achievement;

public class XpSummaryDto
{
    public int TotalXp { get; set; }
    public int Level { get; set; }
    public int XpIntoCurrentLevel { get; set; }
    public int XpForNextLevel { get; set; }
    public int XpToNextLevel { get; set; }
}
