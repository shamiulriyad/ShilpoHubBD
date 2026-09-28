using Microsoft.Extensions.Logging;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.AIShopping;

// Reuses the same real infrastructure as the marketplace's AI product search (IProductSearchCandidateProvider =
// Gemini structured-attribute extraction + real Qdrant candidates over the real product catalog;
// IProductSearchQueryRepository = active+approved products only, hydrated from PostgreSQL) instead of a second
// Gemini client or an invented product list. The only feature-specific logic here is "what family of craft
// complements this one" (jewellery/leather pair with textiles, etc.) and a relevance score over the item's own
// real attributes. Earlier this returned the same 3 fixed mock items regardless of the description given.
public class FashionMatchingService : IFashionMatchingService
{
    private const int MaxSuggestions = 5;

    // Real seeded category slugs (MarketplaceReferenceDataSeeder) grouped by what they typically pair
    // with in an outfit. Matching is by slug, not display name, since slugs are the stable identifier.
    private static readonly string[] TextileSlugs =
    {
        "jamdani-weaving", "dhakai-muslin", "rajshahi-silk", "nakshi-kantha",
        "handloom-textiles", "embroidery", "natural-dye-batik",
    };

    private static readonly string[] JewelrySlugs = { "jewellery-metalwork", "brass-bell-metal" };

    private static readonly string[] BagSlugs = { "leather-craft", "jute-craft", "bamboo-cane" };

    private static readonly string[] UniversalAccessorySlugs =
        { "jewellery-metalwork", "leather-craft", "handloom-textiles" };

    private readonly IProductSearchCandidateProvider _candidateProvider;
    private readonly IProductSearchQueryRepository _queryRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<FashionMatchingService> _logger;

    public FashionMatchingService(
        IProductSearchCandidateProvider candidateProvider,
        IProductSearchQueryRepository queryRepository,
        ICategoryRepository categoryRepository,
        ILogger<FashionMatchingService> logger)
    {
        _candidateProvider = candidateProvider;
        _queryRepository = queryRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<List<FashionMatchDto>> GetMatchesAsync(FashionMatchRequest request, CancellationToken cancellationToken)
    {
        var description = (request.ItemDescription ?? string.Empty).Trim();
        _logger.LogInformation("Fashion match requested. DescriptionLength={Length}", description.Length);

        var categories = await _categoryRepository.GetAllAsync(includeInactive: false, cancellationToken);

        // Real Gemini attribute extraction + real Qdrant search (same pipeline the marketplace's AI product
        // search already uses) -- gives us category/product type/materials/occasions/colors/price range
        // grounded in the user's actual words. Null means the Python service is unavailable; we still have a
        // real (if cruder) path below, never a fake one.
        ProductSearchAnalysisDto? analysis = null;
        if (!string.IsNullOrWhiteSpace(description))
        {
            var candidates = await _candidateProvider.GetCandidatesAsync(description, limit: 20, cancellationToken);
            analysis = candidates?.Analysis;
        }
        _logger.LogInformation(
            "Attribute extraction {Status}. CategorySlug={CategorySlug} Occasions={Occasions} Colors={Colors}",
            analysis is null ? "unavailable (using keyword fallback)" : "succeeded",
            analysis?.CategorySlug, analysis is null ? 0 : analysis.Occasions.Count, analysis is null ? 0 : analysis.Colors.Count);

        // ComplementSlugsFor(null) also resolves to the universal accessory set, so branching on the
        // source category up front (rather than "try the complement search, see if it's empty") is what
        // keeps "couldn't identify anything" and "identified it but no matching stock" from collapsing
        // into the same, mislabelled code path.
        var sourceCategory = ResolveSourceCategory(analysis, description, categories);
        var isFallback = sourceCategory is null;

        List<Product> pool;
        if (isFallback)
        {
            // Nothing could be identified from the input at all -- fall back to popular items from
            // generally complementary craft families, clearly labelled below as a generic pick, not a
            // targeted match.
            var universalCategories = categories.Where(c => UniversalAccessorySlugs.Contains(c.Slug)).ToList();
            pool = await SearchCategoriesAsync(universalCategories, analysis: null, cancellationToken);
        }
        else
        {
            var complementSlugs = ComplementSlugsFor(sourceCategory);
            var complementCategories = categories.Where(c => complementSlugs.Contains(c.Slug) && c.Id != sourceCategory!.Id).ToList();
            pool = await SearchCategoriesAsync(complementCategories, analysis, cancellationToken);

            if (pool.Count == 0)
            {
                // We understood what the item is but have no real complementary stock for it -- an
                // honest "no match" beats inventing an unrelated substitute.
                _logger.LogInformation("Fashion match completed. ResultCount=0 (no complementary stock for the identified category)");
                return new List<FashionMatchDto>();
            }
        }

        var ranked = pool
            .Select(p => (Product: p, Score: RelevanceScore(p, analysis)))
            .OrderByDescending(x => x.Score)
            .Take(MaxSuggestions)
            .ToList();

        _logger.LogInformation("Fashion match completed. ResultCount={Count} UsedFallback={UsedFallback}", ranked.Count, isFallback);

        var itemText = string.IsNullOrWhiteSpace(description) ? "your item" : description;
        return ranked
            .Select(x => new FashionMatchDto
            {
                ProductId = x.Product.Id,
                ItemName = x.Product.Name,
                MatchType = x.Product.Category.Name,
                Reason = BuildReason(x.Product, itemText, isFallback),
            })
            .ToList();
    }

    private async Task<List<Product>> SearchCategoriesAsync(
        List<Category> categories, ProductSearchAnalysisDto? analysis, CancellationToken cancellationToken)
    {
        var pool = new List<Product>();
        foreach (var category in categories)
        {
            var criteria = new ProductSearchCriteria
            {
                CategorySlug = category.Slug,
                MinPrice = analysis?.MinPrice,
                MaxPrice = analysis?.MaxPrice,
                Sort = "rating",
            };

            // take:null -> candidate mode: up to 300 real, active+approved rows for in-memory ranking below.
            var (items, _) = await _queryRepository.SearchAsync(criteria, skip: null, take: null, cancellationToken);
            pool.AddRange(items.Where(p => pool.All(existing => existing.Id != p.Id)));
        }

        return pool;
    }

    private static Category? ResolveSourceCategory(ProductSearchAnalysisDto? analysis, string description, List<Category> categories)
    {
        if (analysis?.CategorySlug is { } slug)
        {
            var match = categories.FirstOrDefault(c => c.Slug == slug);
            if (match is not null) return match;
        }

        return FindMentionedCategory(description, categories);
    }

    private static Category? FindMentionedCategory(string description, List<Category> categories)
    {
        if (string.IsNullOrWhiteSpace(description)) return null;

        return categories.FirstOrDefault(c => c.Name.Split(' ', '&')
            .Where(word => word.Length > 3)
            .Any(word => description.Contains(word, StringComparison.OrdinalIgnoreCase)));
    }

    private static string[] ComplementSlugsFor(Category? sourceCategory)
    {
        if (sourceCategory is null) return UniversalAccessorySlugs;

        if (TextileSlugs.Contains(sourceCategory.Slug)) return JewelrySlugs.Concat(BagSlugs).ToArray();
        if (JewelrySlugs.Contains(sourceCategory.Slug)) return TextileSlugs;
        if (BagSlugs.Contains(sourceCategory.Slug)) return JewelrySlugs.Concat(TextileSlugs.Take(2)).ToArray();

        return UniversalAccessorySlugs;
    }

    // 0..1 blend of real signals only: how well the product's own attributes match what the AI extracted from
    // the description (occasion/color/material overlap), plus quality and popularity -- the same style of
    // relevance score ProductSearchService uses, no invented numbers.
    private static double RelevanceScore(Product product, ProductSearchAnalysisDto? analysis)
    {
        var quality = (double)product.BayesianRating / 5.0;
        var popularity = Math.Min(1.0, Math.Log10(1 + product.SalesCount) / 3.0);
        var bonus = 0.0;

        if (analysis is not null)
        {
            if (product.Attributes is { } attrs)
            {
                if (analysis.Occasions.Count > 0 && attrs.Occasions.Any(o => analysis.Occasions.Contains(o, StringComparer.OrdinalIgnoreCase)))
                {
                    bonus += 0.20;
                }

                if (analysis.Colors.Count > 0 && attrs.Colors.Any(c => analysis.Colors.Contains(c, StringComparer.OrdinalIgnoreCase)))
                {
                    bonus += 0.10;
                }
            }

            if (analysis.Materials.Count > 0 && product.Materials.Any(m => analysis.Materials.Contains(m.Material.Slug)))
            {
                bonus += 0.10;
            }
        }

        return Math.Min(1.0, 0.55 * quality + 0.15 * popularity + bonus);
    }

    private static string BuildReason(Product product, string itemText, bool isFallback)
    {
        var material = product.Materials.Select(m => m.Material?.Name).FirstOrDefault(n => !string.IsNullOrWhiteSpace(n));
        var materialNote = material is null ? string.Empty : $" in {material}";

        return isFallback
            ? $"Popular {product.Category.Name} pick{materialNote} -- we couldn't identify a specific style from your description, so here's a well-rated option."
            : $"A well-rated {product.Category.Name} piece{materialNote} that pairs well with {itemText}.";
    }
}
