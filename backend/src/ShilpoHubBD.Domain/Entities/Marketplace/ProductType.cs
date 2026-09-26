namespace ShilpoHubBD.Domain.Entities.Marketplace;

/// <summary>
/// What kind of object a product is (saree, dupatta, mat, lamp set...). This is the "subcategory" of the craft
/// (<see cref="Category"/>): a craft such as Dhakai Jamdani sells several product types.
/// </summary>
public class ProductType
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Bangla name; used in search text so Bangla queries match.</summary>
    public string? NameBn { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<Product> Products { get; set; } = new List<Product>();
}

/// <summary>A raw material a product is made of (cotton, jute, brass, murta cane...).</summary>
public class Material
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? NameBn { get; set; }
    public string Slug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ProductMaterial> ProductMaterials { get; set; } = new List<ProductMaterial>();
}

/// <summary>Join row: a product is made of a material.</summary>
public class ProductMaterial
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid MaterialId { get; set; }
    public Material Material { get; set; } = null!;
}
