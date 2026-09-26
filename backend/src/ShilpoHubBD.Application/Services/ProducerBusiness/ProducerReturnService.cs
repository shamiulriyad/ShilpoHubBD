using ShilpoHubBD.Application.DTOs.Commerce;
using ShilpoHubBD.Application.DTOs.Logistics;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.Services.ProducerBusiness;

// Return flow for the producer: the customer asks to return a delivered order, the producer accepts or
// rejects it, an accepted return is collected by the logistics partner that delivered the order and
// brought back to the producer, and once it arrives the customer is refunded whatever they paid.
public class ProducerReturnService : IProducerReturnService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IReturnHandlingRepository _returnRepository;
    private readonly IReturnHandlingService _returnService;
    private readonly IOrderService _orderService;
    private readonly IProducerOrderRepository _producerOrderRepository;

    public ProducerReturnService(
        IOrderRepository orderRepository, IPaymentRepository paymentRepository, IReturnHandlingRepository returnRepository,
        IReturnHandlingService returnService, IOrderService orderService, IProducerOrderRepository producerOrderRepository)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _returnRepository = returnRepository;
        _returnService = returnService;
        _orderService = orderService;
        _producerOrderRepository = producerOrderRepository;
    }

    public async Task<List<ProducerReturnDto>> GetReturnsAsync(Guid producerId, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetReturnOrdersForProducerAsync(producerId, cancellationToken);
        var customers = await _producerOrderRepository.GetCustomerInfoAsync(orders.Select(o => o.UserId).Distinct(), cancellationToken);

        var result = new List<ProducerReturnDto>();
        foreach (var order in orders)
        {
            result.Add(await ToDtoAsync(order, producerId, customers, cancellationToken));
        }

        return result;
    }

    public async Task<ProducerReturnDto> AcceptAsync(Guid producerId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await LoadForProducerAsync(producerId, orderId, cancellationToken);
        if (order.Status != OrderStatus.ReturnRequested)
        {
            throw new ConflictException("This order does not have an open return request.");
        }

        if (await _returnRepository.GetByOrderIdAsync(orderId, cancellationToken) is not null)
        {
            throw new ConflictException("A return pickup has already been arranged for this order.");
        }

        var partnerProfileId = await _returnRepository.GetDeliveredShipmentPartnerAsync(orderId, cancellationToken)
            ?? throw new ConflictException(
                "No logistics partner delivered this order through ShilpoHub, so the return cannot be collected automatically. Ask support to handle it.");

        var mine = order.Items.Where(i => i.Product.ProducerId == producerId).ToList();
        var customers = await _producerOrderRepository.GetCustomerInfoAsync(new[] { order.UserId }, cancellationToken);
        var customerName = customers.TryGetValue(order.UserId, out var info) ? info.FullName : order.RecipientName;

        await _returnService.CreateForOrderReturnAsync(partnerProfileId, producerId, new ReturnFromOrderDetails
        {
            OrderId = order.Id,
            CustomerName = customerName,
            CustomerPhone = order.RecipientPhone,
            PickupAddressLine = order.ShippingAddressLine,
            PickupCity = order.ShippingDistrict?.Name ?? "Bangladesh",
            PickupDistrictId = order.ShippingDistrictId,
            ReasonDetail = order.ReturnReason,
            RefundAmount = order.Total,
            Items = mine.Select(i => new ReturnItemInput
            {
                ProductId = i.ProductId,
                Description = i.ProductName,
                Quantity = i.Quantity,
                UnitRefundAmount = i.UnitPrice,
            }).ToList(),
        }, cancellationToken);

        await _orderService.RecordStatusNoteAsync(order.Id, "The producer accepted your return. A logistics partner will collect the goods and bring them back; you are refunded once they arrive.", cancellationToken);

        return await ToDtoAsync((await _orderRepository.GetByIdAsync(orderId, cancellationToken))!, producerId, customers, cancellationToken);
    }

    public async Task<ProducerReturnDto> RejectAsync(Guid producerId, Guid orderId, string? note, CancellationToken cancellationToken)
    {
        var order = await LoadForProducerAsync(producerId, orderId, cancellationToken);
        if (await _returnRepository.GetByOrderIdAsync(orderId, cancellationToken) is not null)
        {
            throw new ConflictException("A return pickup has already been arranged, so it can no longer be rejected.");
        }

        await _orderService.RejectReturnAsync(orderId, new RejectReturnRequest { Note = note }, cancellationToken);

        var customers = await _producerOrderRepository.GetCustomerInfoAsync(new[] { order.UserId }, cancellationToken);
        return await ToDtoAsync((await _orderRepository.GetByIdAsync(orderId, cancellationToken))!, producerId, customers, cancellationToken);
    }

    private async Task<Order> LoadForProducerAsync(Guid producerId, Guid orderId, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(orderId, cancellationToken)
            ?? throw new NotFoundException("Order not found.");

        if (!order.Items.Any(i => i.Product.ProducerId == producerId))
        {
            throw new UnauthorizedAccessException("This order has none of your products.");
        }

        return order;
    }

    private async Task<ProducerReturnDto> ToDtoAsync(
        Order order, Guid producerId, Dictionary<Guid, (string FullName, string Email)> customers, CancellationToken cancellationToken)
    {
        var mine = order.Items.Where(i => i.Product.ProducerId == producerId).ToList();
        var payments = await _paymentRepository.GetByOrderIdAsync(order.Id, cancellationToken);
        var paid = payments
            .Where(p => p.Status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded)
            .Sum(p => p.Amount);
        var ret = await _returnRepository.GetByOrderIdAsync(order.Id, cancellationToken);

        return new ProducerReturnDto
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            OrderStatus = order.Status.ToString(),
            CustomerName = customers.TryGetValue(order.UserId, out var info) ? info.FullName : order.RecipientName,
            CustomerPhone = order.RecipientPhone,
            Reason = order.ReturnReason,
            RequestedAt = order.StatusHistory.Where(e => e.Status == OrderStatus.ReturnRequested).Select(e => (DateTime?)e.CreatedAt).Max() ?? order.UpdatedAt,
            Items = mine.Select(i => new ProducerReturnLineDto { ProductName = i.ProductName, Quantity = i.Quantity, LineTotal = i.LineTotal }).ToList(),
            ItemsAmount = mine.Sum(i => i.LineTotal),
            OrderTotal = order.Total,
            AmountPaid = paid,
            RefundedAmount = order.RefundAmount,
            ReturnReference = ret?.ReferenceCode,
            ReturnStatus = ret?.Status.ToString(),
            CanRespond = order.Status == OrderStatus.ReturnRequested && ret is null,
        };
    }
}
