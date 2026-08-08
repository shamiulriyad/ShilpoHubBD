using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.DTOs.Marketplace;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }
    public int? LowStockThreshold { get; set; }
    public bool IsActive { get; set; }
    public bool IsFeatured { get; set; }
    public string? MakingProcessVideoUrl { get; set; }
    public string? Story { get; set; }
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public HandmadeVerificationStatus HandmadeVerificationStatus { get; set; }
    public string? HandmadeVerifiedByName { get; set; }
    public string? HandmadeVerificationNotes { get; set; }
    public DateTime? HandmadeVerifiedAt { get; set; }

    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;

    public Guid DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;

    public Guid ProducerId { get; set; }
    public string ProducerName { get; set; } = string.Empty;

    public List<string> ImageUrls { get; set; } = new();
    public List<string> ThreeSixtyImageUrls { get; set; } = new();
    public List<ProductVariantDto> Variants { get; set; } = new();
    public List<ProductVideoDto> Videos { get; set; } = new();

    public bool HasCraftStory { get; set; }
    public bool HasProducerStory { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
