namespace ShilpoHubBD.Application.DTOs.Commerce;

public class AddToCartRequest
{
    public Guid ProductId { get; set; }
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}
