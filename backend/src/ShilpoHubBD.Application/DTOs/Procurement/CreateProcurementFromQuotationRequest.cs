namespace ShilpoHubBD.Application.DTOs.Procurement;

public class CreateProcurementFromQuotationRequest
{
    public string? Title { get; set; }
    public decimal? Budget { get; set; }
    public DateTime? DeliveryDeadline { get; set; }
}
