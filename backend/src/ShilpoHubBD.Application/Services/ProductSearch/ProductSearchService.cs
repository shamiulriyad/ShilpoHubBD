using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Services.ProductSearch;

/// <summary>
/// AI-assisted product search. The AI service only proposes WHICH products might match (ids + relevance); every fact
/// shown to the shopper (price, discount, stock, rating, visibility) is read from PostgreSQL here, and the price / stock /
/// rating / location filters are re-applied against PostgreSQL, so a stale vector index can never show a wrong price
/// or an unapproved product.
/// </summary>
public class ProductSearchService : IProductSearchService
{
    private const int CandidateLimit = 60;
    private const double RelevanceFloor = 0.35;     // relative to the best candidate: drops far-off relaxed matches

    private static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "khuje", "dao", "dekhao", "ache", "achhe", "ki", "er", "jonno", "ekta", "gula", "the", "and", "for", "with", "show", "find", "me", "a", "an", "of", "in",
    };

    private readonly IProductSearchCandidateProvider _provider;
    private readonly IProductSearchQueryRepository _repository;

    public ProductSearchService(IProductSearchCandidateProvider provider, IProductSearchQueryRepository repository)
    {
        _provider = provider;
        _repository = repository;
    }

    public async Task<ProductSearchResultDto> SearchAsync(ProductSearchQuery query, CancellationToken cancellationToken)
    {
        var text = query.Q.Trim();
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 50);

        var ai = await _provider.GetCandidatesAsync(text, CandidateLimit, cancellationToken);
        if (ai is null)
        {
            return await KeywordFallbackAsync(text, page, pageSize, cancellationToken);
        }

        var analysis = ai.Analysis;
        var result = new ProductSearchResultDto { Query = text, Page = page, PageSize = pageSize, Interpretation = ToInterpretation(analysis) };

        if (ai.Mode == "filter_only" || ai.Candidates.Count == 0 && string.IsNullOrWhiteSpace(analysis.EnglishQuery))
        {
            // Fully structured request ("highest rated Jamdani", "available in Dhaka"): PostgreSQL alone answers it.
            var criteria = CriteriaFrom(analysis, strict: true);
            var (items, total) = await _repository.SearchAsync(criteria, (page - 1) * pageSize, pageSize, cancellationToken);
            result.Mode = "filter";
            result.TotalCount = total;
            result.Items = items.Select(p => ToItem(p, null, ReasonsFor(p, analysis))).ToList();
            return result;
        }

        var ids = ai.Candidates.Select(c => c.ProductId).ToList();
        var strict = ai.Pass == "strict";
        var (products, _) = await _repository.SearchAsync(CriteriaFrom(analysis, strict, ids), null, null, cancellationToken);
        var relevance = ai.Candidates.ToDictionary(c => c.ProductId, c => c.Score);
        var top = relevance.Count == 0 ? 1.0 : relevance.Values.Max();

        var ranked = products
            .Where(p => relevance.TryGetValue(p.Id, out var score) && score >= top * RelevanceFloor)
            .Select(p => (Product: p, Score: Score(p, relevance[p.Id], analysis)))
            .ToList();

        var ordered = Order(ranked, analysis).ToList();
        result.Mode = "semantic";
        var exact = ordered.Any(x => (analysis.CategorySlug is null || x.Product.Category.Slug == analysis.CategorySlug)
            && (analysis.ProductType is null || x.Product.ProductType?.Slug == analysis.ProductType));
        if (!exact && (analysis.CategorySlug is not null || analysis.ProductType is not null))
        {
            result.Notice = "No exact match for the requested craft or product type; showing the closest products.";
        }

        result.TotalCount = ordered.Count;
        result.Items = ordered.Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => ToItem(x.Product, Math.Round(x.Score, 3), ReasonsFor(x.Product, analysis))).ToList();
        return result;
    }

    // ---- ranking -------------------------------------------------------------------------------------------------

    private static double Score(Product p, double relevance, ProductSearchAnalysisDto a)
    {
        var quality = (double)p.BayesianRating / 5.0;
        var availability = InStock(p) ? 1.0 : p.Attributes?.MadeToOrder == true ? 0.5 : 0.0;
        var popularity = Math.Min(1.0, Math.Log10(1 + p.SalesCount) / 3.0);
        return Math.Min(1.0, 0.60 * relevance + MatchBonus(p, a) + 0.20 * quality + 0.10 * availability + 0.05 * popularity);
    }

    // How well the product's own facts match what was asked for (only counts what the shopper actually mentioned).
    private static double MatchBonus(Product p, ProductSearchAnalysisDto a)
    {
        var bonus = 0.0;
        if (a.CategorySlug is not null && p.Category.Slug == a.CategorySlug) bonus += 0.10;
        if (a.ProductType is not null && p.ProductType?.Slug == a.ProductType) bonus += 0.10;
        if (a.Materials.Count > 0 && p.Materials.Any(m => a.Materials.Contains(m.Material.Slug))) bonus += 0.04;
        if (a.Occasions.Count > 0 && p.Attributes is { } at && at.Occasions.Any(o => a.Occasions.Contains(o, StringComparer.OrdinalIgnoreCase))) bonus += 0.04;
        if (a.Colors.Count > 0 && p.Attributes is { } ac && ac.Colors.Any(c => a.Colors.Contains(c, StringComparer.OrdinalIgnoreCase))) bonus += 0.02;
        return bonus;
    }

    // An explicit sort ("highest rated", "cheapest") orders WITHIN what the shopper asked for: products of the requested craft / type
    // always come before loosely related ones, otherwise "a good Jamdani" would put a 5-star pot above a 4-star Jamdani.
    private static IEnumerable<(Product Product, double Score)> Order(List<(Product Product, double Score)> ranked, ProductSearchAnalysisDto a)
    {
        var exactFirst = ranked.OrderByDescending(x => IsExact(x.Product, a));
        return a.Sort switch
        {
            "rating" => exactFirst.ThenByDescending(x => x.Product.BayesianRating).ThenByDescending(x => x.Product.ReviewCount).ThenByDescending(x => x.Score),
            "price_asc" => exactFirst.ThenBy(x => x.Product.EffectivePrice).ThenByDescending(x => x.Score),
            "price_desc" => exactFirst.ThenByDescending(x => x.Product.EffectivePrice).ThenByDescending(x => x.Score),
            "newest" => exactFirst.ThenByDescending(x => x.Product.CreatedAt).ThenByDescending(x => x.Score),
            "popular" => exactFirst.ThenByDescending(x => x.Product.SalesCount).ThenByDescending(x => x.Score),
            _ => ranked.OrderByDescending(x => x.Score),
        };
    }

    private static bool IsExact(Product p, ProductSearchAnalysisDto a)
        => (a.CategorySlug is null || p.Category.Slug == a.CategorySlug) && (a.ProductType is null || p.ProductType?.Slug == a.ProductType);

    // ---- helpers -------------------------------------------------------------------------------------------------

    private static ProductSearchCriteria CriteriaFrom(ProductSearchAnalysisDto a, bool strict, IReadOnlyCollection<Guid>? ids = null) => new()
    {
        Ids = ids,
        MinPrice = a.MinPrice,
        MaxPrice = a.MaxPrice,
        MinRating = a.MinRating,
        InStockOnly = a.InStockOnly,
        District = a.District,
        Division = a.Division,
        CategorySlug = strict ? a.CategorySlug : null,
        ProductTypeSlug = strict ? a.ProductType : null,
        MaterialSlugs = strict ? a.Materials : Array.Empty<string>(),
        Sort = a.Sort,
    };

    private async Task<ProductSearchResultDto> KeywordFallbackAsync(string text, int page, int pageSize, CancellationToken cancellationToken)
    {
        // The AI service is down or over quota: a plain keyword search still returns real, approved products.
        var keywords = text.Split(new[] { ' ', ',', '.', '?', '!' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length >= 3 && !Stopwords.Contains(w)).Distinct(StringComparer.OrdinalIgnoreCase).Take(6).ToList();
        var (items, total) = await _repository.SearchAsync(
            new ProductSearchCriteria { Keywords = keywords, Sort = "relevance" }, (page - 1) * pageSize, pageSize, cancellationToken);
        return new ProductSearchResultDto
        {
            Query = text, Mode = "fallback", Page = page, PageSize = pageSize, TotalCount = total,
            Items = items.Select(p => ToItem(p, null, new List<string>())).ToList(),
        };
    }

    private static bool InStock(Product p) => p.Stock > 0 || p.Variants.Any(v => v.IsActive && v.Stock > 0);

    private static List<string> ReasonsFor(Product p, ProductSearchAnalysisDto a)
    {
        var reasons = new List<string>();
        if (a.CategorySlug is not null && p.Category.Slug == a.CategorySlug) reasons.Add($"Craft: {p.Category.Name}");
        if (a.ProductType is not null && p.ProductType?.Slug == a.ProductType) reasons.Add($"Type: {p.ProductType.Name}");
        if (a.MaxPrice is { } max) reasons.Add($"Within ৳{max:N0}");
        if (a.MinPrice is { } min) reasons.Add($"Above ৳{min:N0}");
        if (a.District is not null && p.District.Name == a.District) reasons.Add($"From {p.District.Name}");
        if (a.Occasions.Count > 0 && p.Attributes is { } at && at.Occasions.Any(o => a.Occasions.Contains(o, StringComparer.OrdinalIgnoreCase))) reasons.Add("Suits " + string.Join(", ", a.Occasions));
        if (a.InStockOnly && InStock(p)) reasons.Add("In stock");
        if (a.Sort == "rating" && p.ReviewCount > 0) reasons.Add($"Rated {p.AverageRating:0.0} ({p.ReviewCount} reviews)");
        return reasons;
    }

    private static ProductSearchItemDto ToItem(Product p, double? score, List<string> reasons) => new()
    {
        Id = p.Id,
        Slug = p.Slug,
        Name = p.Name,
        PrimaryImageUrl = p.Images.OrderByDescending(i => i.IsPrimary).ThenBy(i => i.DisplayOrder).Select(i => i.ImageUrl).FirstOrDefault(),
        Price = p.Price,
        DiscountPrice = p.DiscountPrice,
        EffectivePrice = p.EffectivePrice,
        AverageRating = p.AverageRating,
        ReviewCount = p.ReviewCount,
        InStock = InStock(p),
        MadeToOrder = p.Attributes?.MadeToOrder ?? false,
        CategoryName = p.Category.Name,
        ProductTypeName = p.ProductType?.Name,
        DistrictName = p.District.Name,
        ProducerName = p.Producer.FullName,
        MatchScore = score,
        Reasons = reasons,
    };

    private static ProductSearchInterpretationDto ToInterpretation(ProductSearchAnalysisDto a) => new()
    {
        EnglishQuery = string.IsNullOrWhiteSpace(a.EnglishQuery) ? null : a.EnglishQuery,
        Sort = a.Sort, MinPrice = a.MinPrice, MaxPrice = a.MaxPrice, MinRating = a.MinRating, InStockOnly = a.InStockOnly,
        District = a.District, Division = a.Division, CategorySlug = a.CategorySlug, ProductType = a.ProductType,
        Materials = a.Materials, Occasions = a.Occasions, Colors = a.Colors, Language = a.Language, Source = a.Source,
    };
}
