namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class IncomeReportEntryDto
{
    public DateTime PeriodStart { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public int ItemsSold { get; set; }
}
