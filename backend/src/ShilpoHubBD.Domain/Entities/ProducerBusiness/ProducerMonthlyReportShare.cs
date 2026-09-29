using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.ProducerBusiness;

/// <summary>
/// Grants a Government/NGO user read access to one ProducerMonthlyReport. Its mere existence is the
/// access check — GetSharedReportAsync/GetPagedAsync (forced to the caller's own id) only ever return
/// reports that have a row here for the requesting user. One row per (ReportId, SharedWithUserId).
/// </summary>
public class ProducerMonthlyReportShare
{
    public Guid Id { get; set; }

    public Guid ReportId { get; set; }
    public ProducerMonthlyReport Report { get; set; } = null!;

    public Guid SharedWithUserId { get; set; }
    public User SharedWithUser { get; set; } = null!;

    public Guid SharedByUserId { get; set; }
    public User SharedByUser { get; set; } = null!;

    public DateTime SharedAt { get; set; }
}
