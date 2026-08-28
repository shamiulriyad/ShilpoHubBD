using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.SupplierDiscovery;

public class SupplierProductSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public string? ImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public HandmadeVerificationStatus HandmadeVerificationStatus { get; set; }
}
