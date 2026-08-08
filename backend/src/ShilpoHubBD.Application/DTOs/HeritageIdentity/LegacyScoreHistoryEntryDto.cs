namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class LegacyScoreHistoryEntryDto
{
    public Guid Id { get; set; }
    public int Score { get; set; }
    public LegacyScoreBreakdownDto Breakdown { get; set; } = new();
    public DateTime CalculatedAt { get; set; }
}
