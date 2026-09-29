namespace ShilpoHubBD.Application.DTOs.ProducerBusiness;

public class ProducerMonthlyReportShareResultDto
{
    public int SharedCount { get; set; }

    /// <summary>(Report, User) pairs that already had a share — left untouched, not duplicated.</summary>
    public int AlreadySharedCount { get; set; }
}
