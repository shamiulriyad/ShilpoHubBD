namespace ShilpoHubBD.Application.DTOs.Procurement;

public class PayProcurementAdvanceRequest
{
    public decimal Amount { get; set; }
    public string? Method { get; set; }
    public string? Reference { get; set; }
}
