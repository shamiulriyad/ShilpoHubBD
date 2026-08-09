namespace ShilpoHubBD.Application.DTOs.Quotations;

public class SubmitQuotationResponseRequest
{
    public decimal TotalPrice { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public List<SubmitQuotationResponseItemInput> Items { get; set; } = new();
}
