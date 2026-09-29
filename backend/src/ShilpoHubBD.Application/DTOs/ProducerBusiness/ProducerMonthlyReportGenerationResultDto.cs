namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerMonthlyReportGenerationResultDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public int ProducerCount { get; set; }
    public int GeneratedCount { get; set; }

    /// <summary>Producers that already had a report for this month — never overwritten.</summary>
    public int SkippedCount { get; set; }
}
