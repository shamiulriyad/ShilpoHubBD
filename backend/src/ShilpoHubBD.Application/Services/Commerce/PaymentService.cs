using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.Commerce;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IEnumerable<IPaymentProvider> _providers;

    public PaymentService(IPaymentRepository paymentRepository, IOrderRepository orderRepository, IEnumerable<IPaymentProvider> providers)
    {
        _paymentRepository = paymentRepository;
        _orderRepository = orderRepository;
        _providers = providers;
    }

    public async Task<PaymentDto> InitiateAsync(Guid orderId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!isAdmin && order.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to pay for this order.");
        }

        if (await _paymentRepository.HasActivePaymentAsync(orderId, cancellationToken))
        {
            throw new ConflictException("This order already has an active or completed payment.");
        }

        var providerName = order.PaymentMethod.ToString();
        var provider = ResolveProvider(providerName);

        var now = DateTime.UtcNow;
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Order = order,
            Provider = providerName,
            Amount = order.Total,
            Status = PaymentStatus.Pending,
            CreatedAt = now,
            UpdatedAt = now,
        };

        var result = await provider.InitiateAsync(payment, cancellationToken);
        ApplyInitiationResult(payment, result);

        await _paymentRepository.AddAsync(payment, cancellationToken);
        await _paymentRepository.SaveChangesAsync(cancellationToken);

        return ToDto(payment);
    }

    public async Task<List<PaymentDto>> GetByOrderIdAsync(Guid orderId, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!isAdmin && order.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to view payments for this order.");
        }

        var payments = await _paymentRepository.GetByOrderIdAsync(orderId, cancellationToken);
        return payments.Select(ToDto).ToList();
    }

    public async Task<PaymentDto> GetByIdAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Payment not found.");

        EnsureOwnershipOrAdmin(payment, currentUserId, isAdmin);
        return ToDto(payment);
    }

    public async Task<PaymentDto> VerifyAsync(Guid id, Guid currentUserId, bool isAdmin, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Payment not found.");

        EnsureOwnershipOrAdmin(payment, currentUserId, isAdmin);

        if (payment.Status is PaymentStatus.Paid or PaymentStatus.Refunded or PaymentStatus.PartiallyRefunded)
        {
            throw new ConflictException($"Payment is already '{payment.Status}'.");
        }

        var provider = ResolveProvider(payment.Provider);
        var result = await provider.VerifyAsync(payment, cancellationToken);
        ApplyVerificationResult(payment, result);

        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(payment);
    }

    public async Task<PaymentDto> HandleCallbackAsync(Guid id, string payload, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Payment not found.");

        var provider = ResolveProvider(payment.Provider);
        var result = await provider.HandleCallbackAsync(payment, payload, cancellationToken);
        ApplyVerificationResult(payment, result);

        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(payment);
    }

    public async Task<PaymentDto> RefundAsync(Guid id, RefundPaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await _paymentRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Payment not found.");

        if (payment.Status != PaymentStatus.Paid && payment.Status != PaymentStatus.PartiallyRefunded)
        {
            throw new ConflictException("Only paid payments can be refunded.");
        }

        var remaining = payment.Amount - payment.RefundedAmount;
        var amount = request.Amount ?? remaining;
        if (amount <= 0 || amount > remaining)
        {
            throw new ConflictException($"Refund amount must be between 0 and {remaining}.");
        }

        var provider = ResolveProvider(payment.Provider);
        var result = await provider.RefundAsync(payment, amount, cancellationToken);
        if (!result.Success)
        {
            throw new ConflictException(result.Message ?? "Refund failed.");
        }

        payment.RefundedAmount += amount;
        payment.Status = payment.RefundedAmount >= payment.Amount ? PaymentStatus.Refunded : PaymentStatus.PartiallyRefunded;
        payment.RefundReason = request.Reason?.Trim();
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.SaveChangesAsync(cancellationToken);
        return ToDto(payment);
    }

    private IPaymentProvider ResolveProvider(string name)
        => _providers.FirstOrDefault(p => p.Name == name)
            ?? throw new NotFoundException($"No payment provider registered for '{name}'.");

    private static void ApplyInitiationResult(Payment payment, PaymentGatewayResult result)
    {
        if (result.Success)
        {
            payment.Status = PaymentStatus.Awaiting;
            payment.TransactionReference = result.TransactionReference;
        }
        else
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = result.Message;
        }
    }

    private static void ApplyVerificationResult(Payment payment, PaymentGatewayResult result)
    {
        payment.UpdatedAt = DateTime.UtcNow;

        if (result.Success)
        {
            payment.Status = PaymentStatus.Paid;
            payment.TransactionReference = result.TransactionReference ?? payment.TransactionReference;
            payment.PaidAt = DateTime.UtcNow;
        }
        else
        {
            payment.FailureReason = result.Message;
        }
    }

    private static void EnsureOwnershipOrAdmin(Payment payment, Guid currentUserId, bool isAdmin)
    {
        if (!isAdmin && payment.Order.UserId != currentUserId)
        {
            throw new UnauthorizedAccessException("You do not have permission to access this payment.");
        }
    }

    private static PaymentDto ToDto(Payment payment) => new()
    {
        Id = payment.Id,
        OrderId = payment.OrderId,
        OrderNumber = payment.Order.OrderNumber,
        Provider = payment.Provider,
        Amount = payment.Amount,
        RefundedAmount = payment.RefundedAmount,
        Status = payment.Status.ToString(),
        TransactionReference = payment.TransactionReference,
        FailureReason = payment.FailureReason,
        RefundReason = payment.RefundReason,
        PaidAt = payment.PaidAt,
        CreatedAt = payment.CreatedAt,
        UpdatedAt = payment.UpdatedAt,
    };
}
