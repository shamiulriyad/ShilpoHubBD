namespace ShilpoHubBD.Domain.Entities.Commerce;

public class Payment
{
    public Guid Id { get; set; }

    public Guid OrderId { get; set; }
    public Order Order { get; set; } = null!;

    // Abstract provider identity (e.g. "CashOnDelivery"); resolved to an IPaymentProvider at runtime.
    public string Provider { get; set; } = string.Empty;

    public decimal Amount { get; set; }
    public decimal RefundedAmount { get; set; }
    public PaymentStatus Status { get; set; }

    public string? TransactionReference { get; set; }
    public string? FailureReason { get; set; }
    public string? RefundReason { get; set; }

    public DateTime? PaidAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
