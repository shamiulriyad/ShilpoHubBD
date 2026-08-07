namespace ShilpoHubBD.Application.Common;

public class PaymentGatewayResult
{
    public bool Success { get; set; }
    public string? TransactionReference { get; set; }
    public string? Message { get; set; }

    public static PaymentGatewayResult Succeeded(string? transactionReference = null, string? message = null)
        => new() { Success = true, TransactionReference = transactionReference, Message = message };

    public static PaymentGatewayResult Failed(string message)
        => new() { Success = false, Message = message };
}
