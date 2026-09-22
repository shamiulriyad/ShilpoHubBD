using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Seed;

public static class MarketplaceReferenceDataSeeder
{
    private static readonly (string Name, string Slug, string Description, string? ImageUrl)[] CraftCategories =
    {
        ("Jamdani Weaving", "jamdani-weaving", "Fine handwoven muslin known for its geometric and floral motifs.", "/images/loom-photo.jpg"),
        ("Nakshi Kantha", "nakshi-kantha", "Traditional embroidered quilts that preserve family stories and regional motifs.", "/images/heritage-crafts.png"),
        ("Pottery & Terracotta", "pottery-terracotta", "Hand-shaped earthenware, terracotta sculpture and household pottery.", "/images/pottery-photo.jpg"),
        ("Jute Craft", "jute-craft", "Baskets, bags, homeware and decorative work made from natural jute fibre.", "/images/learning-together.jpg"),
        ("Bamboo & Cane", "bamboo-cane", "Furniture, baskets and everyday objects woven from bamboo and cane.", "/images/village-community.jpg"),
        ("Handloom Textiles", "handloom-textiles", "Regional saris, lungis, scarves and fabrics woven on traditional looms.", "/images/heritage-weaver.png"),
        ("Shital Pati", "shital-pati", "Cool sleeping mats woven from murta reed using intricate patterns.", "/images/bangladesh-river.jpg"),
        ("Brass & Bell Metal", "brass-bell-metal", "Cast and hammered utensils, ornaments and ceremonial objects.", "/images/festival-community.jpg"),
        ("Wood Carving", "wood-carving", "Architectural details, furniture and decorative objects carved by hand.", "/images/heritage-landscape.png"),
        ("Rickshaw Art", "rickshaw-art", "Bright hand-painted panels and popular visual storytelling.", null),
        ("Folk Painting & Alpana", "folk-painting-alpana", "Painted scrolls, folk imagery and ceremonial floor patterns.", null),
        ("Jewellery & Metalwork", "jewellery-metalwork", "Handcrafted jewellery and decorative metalwork using regional techniques.", null),
        ("Leather Craft", "leather-craft", "Hand-finished bags, footwear and practical leather goods.", null),
        ("Natural Dye & Batik", "natural-dye-batik", "Patterned textiles coloured with wax-resist and natural dye processes.", null),
        ("Embroidery", "embroidery", "Decorative hand stitching used on garments, home textiles and accessories.", null),
        ("Clay Dolls", "clay-dolls", "Painted folk figures and toys shaped from local clay.", null),
    };

    public static async Task SeedCraftCategoriesAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var existingCategories = await context.Categories.ToListAsync(cancellationToken);
        var existingBySlug = existingCategories.ToDictionary(category => category.Slug, StringComparer.OrdinalIgnoreCase);
        var now = DateTime.UtcNow;

        for (var index = 0; index < CraftCategories.Length; index++)
        {
            var item = CraftCategories[index];
            if (existingBySlug.TryGetValue(item.Slug, out var existing))
            {
                if (string.IsNullOrWhiteSpace(existing.Description)) existing.Description = item.Description;
                if (string.IsNullOrWhiteSpace(existing.ImageUrl) && item.ImageUrl is not null) existing.ImageUrl = item.ImageUrl;
                existing.IsActive = true;
                existing.UpdatedAt = now;
                continue;
            }

            context.Categories.Add(new Category
            {
                Id = Guid.NewGuid(),
                Name = item.Name,
                Slug = item.Slug,
                Description = item.Description,
                ImageUrl = item.ImageUrl,
                DisplayOrder = index + 1,
                IsActive = true,
                CreatedAt = now,
                UpdatedAt = now,
            });
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
