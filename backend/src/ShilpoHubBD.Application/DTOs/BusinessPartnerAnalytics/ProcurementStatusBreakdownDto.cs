using ShilpoHubBD.Domain.Entities.Procurement;

namespace ShilpoHubBD.Application.DTOs.BusinessPartnerAnalytics;

public class ProcurementStatusBreakdownDto
{
    public ProcurementStatus Status { get; set; }
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
}
