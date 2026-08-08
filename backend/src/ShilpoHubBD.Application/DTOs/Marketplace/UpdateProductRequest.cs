namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class UpdateProductRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }
    public Guid DistrictId { get; set; }
    public List<string> ImageUrls { get; set; } = new();
    public List<string> ThreeSixtyImageUrls { get; set; } = new();
    public string? MakingProcessVideoUrl { get; set; }
    public string? Story { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
}
