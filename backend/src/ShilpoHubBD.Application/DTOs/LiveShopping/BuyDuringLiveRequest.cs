namespace ShilpoHubBD.Application.DTOs.LiveShopping;

public class BuyDuringLiveRequest
{
    public Guid? ProductVariantId { get; set; }
    public int Quantity { get; set; } = 1;
}
