using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public int Stock { get; set; }

    public bool IsFeatured { get; set; }
    public bool IsActive { get; set; } = true;

    public string? MakingProcessVideoUrl { get; set; }
    public string? Story { get; set; }
    public int? LowStockThreshold { get; set; }

    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }

    public HandmadeVerificationStatus HandmadeVerificationStatus { get; set; } = HandmadeVerificationStatus.Pending;
    public Guid? HandmadeVerifiedByUserId { get; set; }
    public User? HandmadeVerifiedBy { get; set; }
    public string? HandmadeVerificationNotes { get; set; }
    public DateTime? HandmadeVerifiedAt { get; set; }

    /// <summary>Admin listing-approval gate: only <see cref="ProductApprovalStatus.Approved"/> products appear in public storefront queries.</summary>
    public ProductApprovalStatus ApprovalStatus { get; set; } = ProductApprovalStatus.Pending;
    public Guid? ApprovedByUserId { get; set; }
    public User? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public Guid CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public Guid DistrictId { get; set; }
    public District District { get; set; } = null!;

    public Guid ProducerId { get; set; }
    public User Producer { get; set; } = null!;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
}
