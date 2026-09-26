using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Seed;

// Reference data for AI product search: the product-type ("subcategory") lookup and the materials lookup, plus a
// one-time, rule-based backfill of ProductTypeId on existing products and an initial index state per product.
// Lookups are seeded only while their table is EMPTY, so admin edits and deletions are never undone by a restart.
public static class ProductSearchSeeder
{
    private static readonly (string Slug, string Name, string NameBn)[] ProductTypes =
    {
        ("saree", "Saree", "শাড়ি"),
        ("dupatta", "Dupatta", "ওড়না"),
        ("scarf", "Scarf & Stole", "স্কার্ফ"),
        ("shawl", "Shawl", "শাল"),
        ("fabric-length", "Fabric Length", "থান কাপড়"),
        ("panjabi", "Panjabi & Kurta", "পাঞ্জাবি"),
        ("lungi", "Lungi", "লুঙ্গি"),
        ("gamcha", "Gamcha", "গামছা"),
        ("kantha-throw", "Kantha Throw & Bedcover", "কাঁথা"),
        ("cushion-cover", "Cushion Cover", "কুশন কভার"),
        ("wall-hanging", "Wall Hanging", "দেয়াল সজ্জা"),
        ("mat", "Mat (Pati)", "পাটি"),
        ("table-mat", "Table Mat & Runner", "টেবিল ম্যাট"),
        ("basket", "Basket", "ঝুড়ি"),
        ("bag", "Bag & Pouch", "ব্যাগ"),
        ("pot", "Pot & Vase", "মাটির পাত্র"),
        ("terracotta-plaque", "Terracotta Plaque", "টেরাকোটা ফলক"),
        ("doll", "Doll & Toy", "পুতুল"),
        ("lamp", "Lamp & Diya", "প্রদীপ"),
        ("brass-ware", "Brass & Bell-metal Ware", "পিতলের বাসন"),
        ("plate", "Decorative Plate", "সজ্জা থালা"),
        ("jewellery", "Jewellery", "গয়না"),
        ("sculpture", "Sculpture & Carving", "ভাস্কর্য"),
        ("furniture", "Furniture", "আসবাব"),
        ("painting", "Painting & Folk Art", "চিত্রকর্ম"),
        ("footwear", "Footwear", "জুতা"),
        ("accessory", "Accessory", "আনুষঙ্গিক"),
        ("home-decor", "Home Décor (other)", "গৃহসজ্জা"),
        ("other", "Other", "অন্যান্য"),
    };

    private static readonly (string Slug, string Name, string NameBn)[] Materials =
    {
        ("cotton", "Cotton", "সুতি"),
        ("silk", "Silk", "রেশম"),
        ("muslin-yarn", "Fine Muslin Yarn", "মসলিন সুতা"),
        ("jute", "Jute", "পাট"),
        ("bamboo", "Bamboo", "বাঁশ"),
        ("cane", "Cane / Murta", "বেত / মুর্তা"),
        ("sabai-grass", "Sabai Grass", "সাবাই ঘাস"),
        ("banana-fibre", "Banana Fibre", "কলার আঁশ"),
        ("water-hyacinth", "Water Hyacinth", "কচুরিপানা"),
        ("clay", "Clay", "মাটি"),
        ("terracotta", "Terracotta", "টেরাকোটা"),
        ("brass", "Brass", "পিতল"),
        ("bell-metal", "Bell Metal", "কাঁসা"),
        ("wood", "Wood", "কাঠ"),
        ("leather", "Leather", "চামড়া"),
        ("natural-dye", "Natural Dye", "প্রাকৃতিক রং"),
        ("zari", "Zari Thread", "জরি"),
        ("recycled-fabric", "Recycled Fabric", "পুরনো কাপড়"),
        ("shell", "Shell", "শঙ্খ / ঝিনুক"),
        ("coconut-shell", "Coconut Shell", "নারকেলের খোল"),
        ("paper", "Handmade Paper", "হাতে তৈরি কাগজ"),
        ("stone", "Stone", "পাথর"),
    };

    // Ordered: first match wins. Matched against the product name and craft (category) name.
    private static readonly (string Pattern, string Slug)[] TypeRules =
    {
        (@"sar?ee|shari|sari|শাড়ি", "saree"),
        (@"dupatta|orna|ওড়না", "dupatta"),
        (@"scarf|stole", "scarf"),
        (@"shawl", "shawl"),
        (@"panjabi|punjabi|kurta", "panjabi"),
        (@"lungi", "lungi"),
        (@"gamcha|gamchha", "gamcha"),
        (@"kantha|katha|kanatha|কাঁথা", "kantha-throw"),
        (@"cushion", "cushion-cover"),
        (@"table ?mat|runner", "table-mat"),
        (@"\bmat\b|pati\b|পাটি", "mat"),
        (@"basket|jhuri|tokri|ঝুড়ি", "basket"),
        (@"\bbag\b|purse|pouch|ব্যাগ", "bag"),
        (@"terracotta plaque|plaque", "terracotta-plaque"),
        (@"vase|jar\b|matka|\bpot\b", "pot"),
        (@"doll|putul|পুতুল", "doll"),
        (@"diya|lamp|prodip|প্রদীপ", "lamp"),
        (@"plate|thali", "plate"),
        (@"jewel|necklace|earring|bangle|গয়না", "jewellery"),
        (@"sculpture|carving|statue", "sculpture"),
        (@"painting|alpana|chitra|scroll", "painting"),
        (@"sandal|shoe|slipper", "footwear"),
    };

    public static async Task SeedAsync(ShilpoHubDbContext context, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        if (!await context.ProductTypes.AnyAsync(cancellationToken))
        {
            context.ProductTypes.AddRange(ProductTypes.Select((t, i) => new ProductType
            {
                Id = Guid.NewGuid(), Slug = t.Slug, Name = t.Name, NameBn = t.NameBn, DisplayOrder = i, IsActive = true, CreatedAt = now, UpdatedAt = now,
            }));
            await context.SaveChangesAsync(cancellationToken);
        }

        if (!await context.Materials.AnyAsync(cancellationToken))
        {
            context.Materials.AddRange(Materials.Select((m, i) => new Material
            {
                Id = Guid.NewGuid(), Slug = m.Slug, Name = m.Name, NameBn = m.NameBn, DisplayOrder = i, IsActive = true, CreatedAt = now, UpdatedAt = now,
            }));
            await context.SaveChangesAsync(cancellationToken);
        }

        await BackfillProductTypesAsync(context, cancellationToken);

        // Every product needs an index state so the first worker run picks it up (new products get one from the interceptor).
        await context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""ProductIndexStates"" (""ProductId"", ""Status"", ""Version"", ""AttemptCount"", ""UpdatedAt"")
            SELECT p.""Id"", 'Dirty', 1, 0, {now} FROM ""Products"" p
            WHERE NOT EXISTS (SELECT 1 FROM ""ProductIndexStates"" s WHERE s.""ProductId"" = p.""Id"")", cancellationToken);
    }

    // Conservative: only products with no type yet, only on a clear keyword match. Producers can change it afterwards.
    private static async Task BackfillProductTypesAsync(ShilpoHubDbContext context, CancellationToken cancellationToken)
    {
        var untyped = await context.Products.Include(p => p.Category).Where(p => p.ProductTypeId == null).ToListAsync(cancellationToken);
        if (untyped.Count == 0)
        {
            return;
        }

        var types = await context.ProductTypes.ToDictionaryAsync(t => t.Slug, t => t.Id, cancellationToken);
        var rules = TypeRules.Select(r => (Regex: new Regex(r.Pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant), r.Slug)).ToList();

        foreach (var product in untyped)
        {
            var text = $"{product.Name} {product.Category.Name}";
            var match = rules.FirstOrDefault(r => r.Regex.IsMatch(text));
            if (match.Regex is not null && types.TryGetValue(match.Slug, out var typeId))
            {
                product.ProductTypeId = typeId;
            }
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
