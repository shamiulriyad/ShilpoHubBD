namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class IncomeReportQueryParameters
{
    public DateTime FromDate { get; set; } = DateTime.UtcNow.AddMonths(-1);
    public DateTime ToDate { get; set; } = DateTime.UtcNow;
    public IncomeReportGroupBy GroupBy { get; set; } = IncomeReportGroupBy.Day;
}
