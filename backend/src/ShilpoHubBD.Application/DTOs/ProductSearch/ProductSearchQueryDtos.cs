using System.Text.Json.Serialization;

namespace ShilpoHubBD.Application.DTOs.ProductSearch;

/// <summary>GET api/product-search?q=...&amp;page=&amp;pageSize=</summary>
public class ProductSearchQuery
{
    public string Q { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 12;
}

/// <summary>How the question was understood; shown to the shopper so they can see why they got these results.</summary>
public class ProductSearchInterpretationDto
{
    public string? EnglishQuery { get; set; }
    public string Sort { get; set; } = "relevance";
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public bool InStockOnly { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public string? CategorySlug { get; set; }
    public string? ProductType { get; set; }
    public List<string> Materials { get; set; } = new();
    public List<string> Occasions { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public string Language { get; set; } = "en";
    public string Source { get; set; } = string.Empty;
}

public class ProductSearchItemDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? PrimaryImageUrl { get; set; }

    // Read from PostgreSQL at request time (never from the vector index).
    public decimal Price { get; set; }
    public decimal? DiscountPrice { get; set; }
    public decimal EffectivePrice { get; set; }
    public string Currency { get; set; } = "BDT";
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public bool InStock { get; set; }
    public bool MadeToOrder { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string? ProductTypeName { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string ProducerName { get; set; } = string.Empty;

    /// <summary>0..1 blend of semantic relevance, attribute matches, quality and availability; null for filter-only answers.</summary>
    public double? MatchScore { get; set; }
    public List<string> Reasons { get; set; } = new();
}

public class ProductSearchResultDto
{
    public string Query { get; set; } = string.Empty;

    /// <summary>semantic = vector retrieval + PostgreSQL re-check; filter = structured filters only; fallback = keyword search (AI service unavailable).</summary>
    public string Mode { get; set; } = "semantic";
    public ProductSearchInterpretationDto? Interpretation { get; set; }

    /// <summary>Set when nothing matched the requested craft / product type exactly and the closest products are shown instead.</summary>
    public string? Notice { get; set; }
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public List<ProductSearchItemDto> Items { get; set; } = new();
}

// ---- what the Python product-search service returns (snake_case for the analysis, camelCase for candidates) ----

public class ProductSearchAnalysisDto
{
    [JsonPropertyName("english_query")] public string? EnglishQuery { get; set; }
    [JsonPropertyName("semantic")] public bool Semantic { get; set; } = true;
    [JsonPropertyName("sort")] public string Sort { get; set; } = "relevance";
    [JsonPropertyName("min_price")] public decimal? MinPrice { get; set; }
    [JsonPropertyName("max_price")] public decimal? MaxPrice { get; set; }
    [JsonPropertyName("min_rating")] public decimal? MinRating { get; set; }
    [JsonPropertyName("in_stock_only")] public bool InStockOnly { get; set; }
    [JsonPropertyName("district")] public string? District { get; set; }
    [JsonPropertyName("division")] public string? Division { get; set; }
    [JsonPropertyName("category_slug")] public string? CategorySlug { get; set; }
    [JsonPropertyName("product_type")] public string? ProductType { get; set; }
    [JsonPropertyName("materials")] public List<string> Materials { get; set; } = new();
    [JsonPropertyName("occasions")] public List<string> Occasions { get; set; } = new();
    [JsonPropertyName("colors")] public List<string> Colors { get; set; } = new();
    [JsonPropertyName("language")] public string Language { get; set; } = "en";
    [JsonPropertyName("source")] public string Source { get; set; } = string.Empty;
}

public class ProductSearchCandidateDto
{
    [JsonPropertyName("productId")] public Guid ProductId { get; set; }
    [JsonPropertyName("score")] public double Score { get; set; }
    [JsonPropertyName("semantic")] public double Semantic { get; set; }
}

public class ProductSearchCandidatesDto
{
    [JsonPropertyName("analysis")] public ProductSearchAnalysisDto Analysis { get; set; } = new();

    /// <summary>semantic | filter_only</summary>
    [JsonPropertyName("mode")] public string Mode { get; set; } = "semantic";

    /// <summary>strict | relaxed | none</summary>
    [JsonPropertyName("pass")] public string Pass { get; set; } = "none";
    [JsonPropertyName("candidates")] public List<ProductSearchCandidateDto> Candidates { get; set; } = new();
}

/// <summary>Hard, authoritative filters applied in PostgreSQL. Category / type / material are only hard on a strict pass or a filter-only answer.</summary>
public class ProductSearchCriteria
{
    public IReadOnlyCollection<Guid>? Ids { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public decimal? MinRating { get; set; }
    public bool InStockOnly { get; set; }
    public string? District { get; set; }
    public string? Division { get; set; }
    public string? CategorySlug { get; set; }
    public string? ProductTypeSlug { get; set; }
    public IReadOnlyCollection<string> MaterialSlugs { get; set; } = Array.Empty<string>();

    /// <summary>Keyword fallback only (AI service unavailable).</summary>
    public IReadOnlyCollection<string> Keywords { get; set; } = Array.Empty<string>();
    public string Sort { get; set; } = "relevance";
}
