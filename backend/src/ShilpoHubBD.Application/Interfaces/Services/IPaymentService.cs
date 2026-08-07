using ShilpoHubBD.Application.DTOs.Commerce;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<PaymentDto> InitiateAsync(Guid orderId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<List<PaymentDto>> GetByOrderIdAsync(Guid orderId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<PaymentDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<PaymentDto> VerifyAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken);
    Task<PaymentDto> HandleCallbackAsync(Guid id, string payload, CancellationToken cancellationToken);
    Task<PaymentDto> RefundAsync(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken);
}
