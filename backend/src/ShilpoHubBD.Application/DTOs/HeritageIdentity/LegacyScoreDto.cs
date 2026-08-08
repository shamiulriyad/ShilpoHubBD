namespace ShilpoHubBD.Application.DTOs.HeritageIdentity;

public class LegacyScoreDto
{
    public Guid ProducerId { get; set; }
    public int Score { get; set; }
    public LegacyScoreBreakdownDto Breakdown { get; set; } = new();
    public DateTime? CalculatedAt { get; set; }
}
