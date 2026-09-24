namespace ShilpoHubBD.Application.DTOs.Logistics;

// What a producer sees when choosing a logistics partner to hand a shipment to.
public class LogisticsPartnerDirectoryItemDto
{
    public Guid ProfileId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string BaseCity { get; set; } = string.Empty;
    public string? BaseDistrictName { get; set; }
    public bool OffersCashOnDelivery { get; set; }
    public bool OffersFragileHandling { get; set; }
}
