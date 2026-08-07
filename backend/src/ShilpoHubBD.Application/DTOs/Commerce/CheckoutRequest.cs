using ShilpoHubBD.Domain.Entities.Commerce;

namespace ShilpoHubBD.Application.DTOs.Commerce;

public class CheckoutRequest
{
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ShippingAddressLine { get; set; } = string.Empty;
    public Guid ShippingDistrictId { get; set; }
    public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.CashOnDelivery;
}
