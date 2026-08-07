namespace ShilpoHubBD.Application.DTOs.Commerce;

public class RefundOrderRequest
{
    public decimal? Amount { get; set; }
    public string? Reason { get; set; }
}
