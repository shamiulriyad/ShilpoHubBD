using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Infrastructure.Payments;

public class CashOnDeliveryPaymentProvider : IPaymentProvider
{
    public string Name { get; } = PaymentMethod.CashOnDelivery.ToString();

    public Task<PaymentGatewayResult> InitiateAsync(Payment payment, CancellationToken cancellationToken)
        => Task.FromResult(PaymentGatewayResult.Succeeded(
            transactionReference: $"COD-{payment.OrderId:N}".ToUpperInvariant(),
            message: "Cash will be collected on delivery."));

    public Task<PaymentGatewayResult> VerifyAsync(Payment payment, CancellationToken cancellationToken)
    {
        if (payment.Order.Status != OrderStatus.Delivered)
        {
            return Task.FromResult(PaymentGatewayResult.Failed(
                "Cash on delivery cannot be verified until the order has been delivered."));
        }

        return Task.FromResult(PaymentGatewayResult.Succeeded(
            transactionReference: payment.TransactionReference,
            message: "Cash collected on delivery."));
    }

    public Task<PaymentGatewayResult> HandleCallbackAsync(Payment payment, string payload, CancellationToken cancellationToken)
        => Task.FromResult(PaymentGatewayResult.Failed("Cash on delivery does not support gateway callbacks."));

    public Task<PaymentGatewayResult> RefundAsync(Payment payment, decimal amount, CancellationToken cancellationToken)
        => Task.FromResult(PaymentGatewayResult.Succeeded(
            transactionReference: $"COD-REFUND-{Guid.NewGuid():N}".ToUpperInvariant(),
            message: "Cash refund recorded manually."));
}
