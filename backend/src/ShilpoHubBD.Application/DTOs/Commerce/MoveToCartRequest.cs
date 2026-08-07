namespace ShilpoHubBD.Application.DTOs.Commerce;

public class MoveToCartRequest
{
    public int Quantity { get; set; } = 1;
    public Guid? ProductVariantId { get; set; }
}
