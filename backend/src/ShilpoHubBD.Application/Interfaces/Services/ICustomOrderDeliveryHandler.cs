namespace ShilpoHubBD.Application.Interfaces.Services;

// Called by delivery tracking when a shipment is delivered, so a custom order that was handed to
// logistics under that tracking number becomes Delivered.
public interface ICustomOrderDeliveryHandler
{
    Task HandleShipmentDeliveredAsync(string trackingNumber, CancellationToken cancellationToken);
}
