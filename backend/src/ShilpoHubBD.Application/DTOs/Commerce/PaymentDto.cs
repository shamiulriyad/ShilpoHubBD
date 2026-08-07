namespace ShilpoHubBD.Application.DTOs.Commerce;

public class PaymentDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public string? RefundReason { get; set; }
    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
