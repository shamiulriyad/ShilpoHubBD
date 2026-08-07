using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over a payment gateway. Add a new implementation (e.g. bKash, Stripe) and register it
// in Infrastructure/DependencyInjection.cs to support it -- no changes needed in PaymentService.
public interface IPaymentProvider
{
    string Name { get; }

    Task<PaymentGatewayResult> InitiateAsync(Payment payment, CancellationToken cancellationToken);
    Task<PaymentGatewayResult> VerifyAsync(Payment payment, CancellationToken cancellationToken);
    Task<PaymentGatewayResult> HandleCallbackAsync(Payment payment, string payload, CancellationToken cancellationToken);
    Task<PaymentGatewayResult> RefundAsync(Payment payment, decimal amount, CancellationToken cancellationToken);
}
