using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.CustomOrders;

namespace ShilpoHubBD.Application.Services.CustomOrders;

public class CustomOrderDeliveryHandler : ICustomOrderDeliveryHandler
{
    private readonly ICustomOrderRepository _repository;

    public CustomOrderDeliveryHandler(ICustomOrderRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleShipmentDeliveredAsync(string trackingNumber, CancellationToken cancellationToken)
    {
        var order = await _repository.GetByTrackingNumberAsync(trackingNumber, cancellationToken);
        if (order is null || order.Status != CustomOrderStatus.Shipped)
        {
            return;
        }

        var now = DateTime.UtcNow;
        order.Status = CustomOrderStatus.Delivered;
        order.DeliveredAt = now;
        order.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
