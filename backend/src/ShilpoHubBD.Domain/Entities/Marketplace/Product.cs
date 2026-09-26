using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Domain.Entities.Marketplace;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }

    /// <summary>Database-generated: <c>COALESCE(DiscountPrice, Price)</c>. Read-only; indexed for price filters and sorting.</summary>
    public decimal EffectivePrice { get; private set; }
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

    /// <summary>Database-generated Bayesian average of <see cref="AverageRating"/> (prior 4.0 worth 5 reviews), so "highest rated" is not won by a single 5-star review.</summary>
    public decimal BayesianRating { get; private set; }

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

    /// <summary>The kind of object (saree, mat, lamp set...). Null until backfilled or set by the producer.</summary>
    public Guid? ProductTypeId { get; set; }
    public ProductType? ProductType { get; set; }

    public ProductAttributes? Attributes { get; set; }
    public ICollection<ProductMaterial> Materials { get; set; } = new List<ProductMaterial>();

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    public ICollection<ProductVideo> Videos { get; set; } = new List<ProductVideo>();
}
