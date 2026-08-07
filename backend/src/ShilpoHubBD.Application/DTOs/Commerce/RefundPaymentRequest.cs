namespace ShilpoHubBD.Application.DTOs.Commerce;

public class RefundPaymentRequest
{
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}
