namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerMonthlyReportShareDto
{
    public Guid Id { get; set; }
    public Guid ReportId { get; set; }

    public Guid SharedWithUserId { get; set; }
    public string SharedWithName { get; set; } = string.Empty;
    public string SharedWithEmail { get; set; } = string.Empty;

    public Guid SharedByUserId { get; set; }
    public string SharedByName { get; set; } = string.Empty;

    public DateTime SharedAt { get; set; }
}
