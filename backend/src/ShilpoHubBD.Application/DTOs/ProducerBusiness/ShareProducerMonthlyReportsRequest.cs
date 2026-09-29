namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ShareProducerMonthlyReportsRequest
{
    public List<Guid> ReportIds { get; set; } = new();

    /// <summary>Must each hold the GovernmentNGO role.</summary>
    public List<Guid> SharedWithUserIds { get; set; } = new();
}
