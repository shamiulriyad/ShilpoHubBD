using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NpgsqlTypes;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Data.Configurations;

// Product AI-search schema. Kept out of ProductConfiguration on purpose so the existing product mapping is untouched;
// EF applies every IEntityTypeConfiguration<Product> it finds.

/// <summary>Search-related columns and indexes on <c>Products</c>.</summary>
public class ProductSearchColumnsConfiguration : IEntityTypeConfiguration<Product>
{
    private const string Storefront = "\"IsActive\" AND \"ApprovalStatus\" = 'Approved'";

    public void Configure(EntityTypeBuilder<Product> builder)
    {
        // COALESCE(DiscountPrice, Price): the price a customer actually pays. Generated so it can never drift.
        builder.Property(p => p.EffectivePrice)
            .HasColumnType("decimal(10,2)")
            .HasComputedColumnSql("COALESCE(\"DiscountPrice\", \"Price\")", stored: true);

        // Bayesian average: (prior 4.0 x 5 reviews + sum of ratings) / (5 + review count).
        builder.Property(p => p.BayesianRating)
            .HasColumnType("decimal(3,2)")
            .HasComputedColumnSql("ROUND((20.0 + \"ReviewCount\" * \"AverageRating\") / (5 + \"ReviewCount\"), 2)", stored: true);

        // Keep the plain FK indexes EF generated before these composite ones existed (joins and FK checks cover inactive products too).
        builder.HasIndex(p => p.CategoryId);
        builder.HasIndex(p => p.DistrictId);

        // Real full-text index (the search provider used to compute to_tsvector per row with no index).
        builder.Property<NpgsqlTsVector>("SearchVector")
            .HasComputedColumnSql(
                "to_tsvector('english', coalesce(\"Name\", '') || ' ' || coalesce(\"Description\", '') || ' ' || coalesce(\"Story\", ''))",
                stored: true);
        builder.HasIndex("SearchVector").HasMethod("GIN");

        // Typo-tolerant name matching (needs the pg_trgm extension, created in the migration).
        builder.HasIndex(p => p.Name).HasMethod("gin").HasOperators("gin_trgm_ops").HasDatabaseName("IX_Products_Name_trgm");

        // Public-storefront partial indexes: filters and sorts the AI search re-applies against PostgreSQL.
        builder.HasIndex(p => new { p.CategoryId, p.EffectivePrice }).HasFilter(Storefront).HasDatabaseName("IX_Products_Storefront_Category_Price");
        builder.HasIndex(p => new { p.DistrictId, p.EffectivePrice }).HasFilter(Storefront).HasDatabaseName("IX_Products_Storefront_District_Price");
        builder.HasIndex(p => new { p.ProductTypeId, p.EffectivePrice }).HasFilter(Storefront).HasDatabaseName("IX_Products_Storefront_Type_Price");
        builder.HasIndex(p => p.BayesianRating).IsDescending().HasFilter(Storefront).HasDatabaseName("IX_Products_Storefront_Rating");
        builder.HasIndex(p => p.CreatedAt).IsDescending().HasFilter(Storefront).HasDatabaseName("IX_Products_Storefront_Newest");
        builder.HasIndex(p => p.SalesCount).IsDescending().HasFilter(Storefront).HasDatabaseName("IX_Products_Storefront_Sales");
        builder.HasIndex(p => p.EffectivePrice).HasFilter(Storefront + " AND \"Stock\" > 0").HasDatabaseName("IX_Products_Storefront_InStock_Price");

        builder.HasOne(p => p.ProductType)
            .WithMany(t => t.Products)
            .HasForeignKey(p => p.ProductTypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(p => p.Attributes)
            .WithOne(a => a.Product)
            .HasForeignKey<ProductAttributes>(a => a.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(p => p.Materials)
            .WithOne(m => m.Product)
            .HasForeignKey(m => m.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ProductTypeConfiguration : IEntityTypeConfiguration<ProductType>
{
    public void Configure(EntityTypeBuilder<ProductType> builder)
    {
        builder.ToTable("ProductTypes");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Name).IsRequired().HasMaxLength(120);
        builder.Property(t => t.NameBn).HasMaxLength(120);
        builder.Property(t => t.Slug).IsRequired().HasMaxLength(120);
        builder.Property(t => t.CreatedAt).IsRequired();
        builder.Property(t => t.UpdatedAt).IsRequired();
        builder.HasIndex(t => t.Slug).IsUnique();
        builder.HasIndex(t => new { t.IsActive, t.DisplayOrder });
    }
}

public class MaterialConfiguration : IEntityTypeConfiguration<Material>
{
    public void Configure(EntityTypeBuilder<Material> builder)
    {
        builder.ToTable("Materials");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Name).IsRequired().HasMaxLength(120);
        builder.Property(m => m.NameBn).HasMaxLength(120);
        builder.Property(m => m.Slug).IsRequired().HasMaxLength(120);
        builder.Property(m => m.CreatedAt).IsRequired();
        builder.Property(m => m.UpdatedAt).IsRequired();
        builder.HasIndex(m => m.Slug).IsUnique();
        builder.HasIndex(m => new { m.IsActive, m.DisplayOrder });
    }
}

public class ProductMaterialConfiguration : IEntityTypeConfiguration<ProductMaterial>
{
    public void Configure(EntityTypeBuilder<ProductMaterial> builder)
    {
        builder.ToTable("ProductMaterials");
        builder.HasKey(m => new { m.ProductId, m.MaterialId });
        builder.HasIndex(m => new { m.MaterialId, m.ProductId });
        builder.HasOne(m => m.Material)
            .WithMany(x => x.ProductMaterials)
            .HasForeignKey(m => m.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class ProductAttributesConfiguration : IEntityTypeConfiguration<ProductAttributes>
{
    public void Configure(EntityTypeBuilder<ProductAttributes> builder)
    {
        builder.ToTable("ProductAttributes");
        builder.HasKey(a => a.ProductId);

        builder.Property(a => a.Tags).HasColumnType("text[]").IsRequired();
        builder.Property(a => a.Keywords).HasColumnType("text[]").IsRequired();
        builder.Property(a => a.Occasions).HasColumnType("text[]").IsRequired();
        builder.Property(a => a.Colors).HasColumnType("text[]").IsRequired();

        builder.Property(a => a.CraftTechnique).HasMaxLength(200);
        builder.Property(a => a.ProductionMethod).HasMaxLength(30);
        builder.Property(a => a.DimensionsText).HasMaxLength(120);
        builder.Property(a => a.LengthCm).HasColumnType("decimal(8,2)");
        builder.Property(a => a.WidthCm).HasColumnType("decimal(8,2)");
        builder.Property(a => a.HeightCm).HasColumnType("decimal(8,2)");
        builder.Property(a => a.WeightGrams).HasColumnType("decimal(10,2)");
        builder.Property(a => a.CareInstructions).HasMaxLength(500);
        builder.Property(a => a.Source).IsRequired().HasMaxLength(20);
        builder.Property(a => a.CreatedAt).IsRequired();
        builder.Property(a => a.UpdatedAt).IsRequired();

        builder.HasIndex(a => a.Tags).HasMethod("gin");
        builder.HasIndex(a => a.Keywords).HasMethod("gin");
        builder.HasIndex(a => a.Occasions).HasMethod("gin");
        builder.HasIndex(a => a.Colors).HasMethod("gin");
    }
}

public class ProductAttributeSuggestionConfiguration : IEntityTypeConfiguration<ProductAttributeSuggestion>
{
    public void Configure(EntityTypeBuilder<ProductAttributeSuggestion> builder)
    {
        builder.ToTable("ProductAttributeSuggestions");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.PayloadJson).IsRequired().HasColumnType("jsonb");
        builder.Property(s => s.Model).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.CreatedAt).IsRequired();

        builder.HasIndex(s => new { s.ProductId, s.Status });

        builder.HasOne(s => s.Product)
            .WithMany()
            .HasForeignKey(s => s.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(s => s.ReviewedBy)
            .WithMany()
            .HasForeignKey(s => s.ReviewedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProductIndexStateConfiguration : IEntityTypeConfiguration<ProductIndexState>
{
    public void Configure(EntityTypeBuilder<ProductIndexState> builder)
    {
        builder.ToTable("ProductIndexStates");
        builder.HasKey(s => s.ProductId);
        builder.Property(s => s.ProductId).ValueGeneratedNever();   // = the product id; deliberately no FK (survives product deletion)
        builder.Property(s => s.Status).IsRequired().HasMaxLength(20);
        builder.Property(s => s.SearchTextHash).HasMaxLength(64);
        builder.Property(s => s.EmbeddingModel).HasMaxLength(100);
        builder.Property(s => s.LastError).HasMaxLength(1000);
        builder.Property(s => s.UpdatedAt).IsRequired();

        // The worker only polls unsynced rows.
        builder.HasIndex(s => new { s.Status, s.UpdatedAt }).HasFilter("\"Status\" <> 'Synced'").HasDatabaseName("IX_ProductIndexStates_Pending");
    }
}
