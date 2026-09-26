// Regression checks for ProductSearchService (no database, no AI service): run with `dotnet run`.
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.ProductSearch;
using ShilpoHubBD.Domain.Entities.Marketplace;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

Product Make(string name, decimal price, decimal rating, int reviews, int stock, string category = "jamdani-weaving", string? type = "saree", string district = "Dhaka")
{
    var p = new Product
    {
        Id = Guid.NewGuid(), Name = name, Slug = name.ToLowerInvariant().Replace(' ', '-'), Price = price, Stock = stock, AverageRating = rating, ReviewCount = reviews,
        Category = new Category { Slug = category, Name = category }, District = new District { Name = district, Division = "Dhaka" },
        Producer = new ShilpoHubBD.Domain.Entities.Identity.User { FullName = "Producer" },
        ProductType = type is null ? null : new ProductType { Slug = type, Name = type },
    };
    typeof(Product).GetProperty(nameof(Product.EffectivePrice))!.SetValue(p, price);
    typeof(Product).GetProperty(nameof(Product.BayesianRating))!.SetValue(p, Math.Round((20m + reviews * rating) / (5 + reviews), 2));
    return p;
}

ProductSearchCandidatesDto Ai(string mode, string pass, ProductSearchAnalysisDto a, params (Product P, double S)[] c) => new()
{
    Mode = mode, Pass = pass, Analysis = a, Candidates = c.Select(x => new ProductSearchCandidateDto { ProductId = x.P.Id, Score = x.S }).ToList(),
};

var strong = Make("Jamdani Saree", 9000, 4.8m, 120, 5);
var weak = Make("Plain Saree", 4000, 3.0m, 2, 5);
var otherCraft = Make("Shital Pati", 2000, 4.5m, 30, 5, "shital-pati", "mat", "Sylhet");
var unrelated = Make("Far Off", 1000, 4.0m, 10, 5, "jute-craft", "bag");

// 1. relevance + attribute match beat a slightly better rating
{
    var a = new ProductSearchAnalysisDto { EnglishQuery = "jamdani saree", CategorySlug = "jamdani-weaving", ProductType = "saree" };
    var repo = new FakeRepo(new[] { strong, weak, otherCraft });
    var svc = new ProductSearchService(new FakeProvider(Ai("semantic", "strict", a, (strong, 1.0), (weak, 0.9), (otherCraft, 0.5))), repo);
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "jamdani saree" }, CancellationToken.None);
    Check("semantic ranks the exact craft+type match first", r.Items[0].Id == strong.Id);
    Check("semantic mode reported", r.Mode == "semantic");
    Check("strict pass applies craft/type as hard DB filters", repo.LastCriteria!.CategorySlug == "jamdani-weaving" && repo.LastCriteria.ProductTypeSlug == "saree");
    Check("candidate ids are re-checked in PostgreSQL", repo.LastCriteria.Ids!.Count == 3);
    Check("no notice when an exact match exists", r.Notice is null);
}

// 2. relaxed pass: craft/type are NOT hard filters, and the shopper is told there was no exact match
{
    var a = new ProductSearchAnalysisDto { EnglishQuery = "wedding saree", ProductType = "saree" };
    var repo = new FakeRepo(new[] { otherCraft });
    var svc = new ProductSearchService(new FakeProvider(Ai("semantic", "relaxed", a, (otherCraft, 0.8))), repo);
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "wedding saree" }, CancellationToken.None);
    Check("relaxed pass drops craft/type hard filters", repo.LastCriteria!.ProductTypeSlug is null && repo.LastCriteria.CategorySlug is null);
    Check("notice explains the closest-match fallback", r.Notice is not null && r.Items.Count == 1);
}

// 3. far-off candidates are cut by the relevance floor
{
    var a = new ProductSearchAnalysisDto { EnglishQuery = "jamdani" };
    var repo = new FakeRepo(new[] { strong, unrelated });
    var svc = new ProductSearchService(new FakeProvider(Ai("semantic", "strict", a, (strong, 1.0), (unrelated, 0.2))), repo);
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "jamdani" }, CancellationToken.None);
    Check("candidates below the relevance floor are dropped", r.Items.Count == 1 && r.Items[0].Id == strong.Id);
}

// 4. explicit sort overrides relevance
{
    var a = new ProductSearchAnalysisDto { EnglishQuery = "saree", Sort = "price_asc" };
    var repo = new FakeRepo(new[] { strong, weak });
    var svc = new ProductSearchService(new FakeProvider(Ai("semantic", "strict", a, (strong, 1.0), (weak, 0.9))), repo);
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "saree cheapest" }, CancellationToken.None);
    Check("price_asc sorts by real PostgreSQL price", r.Items[0].Id == weak.Id);
}

// 4b. explicit sort keeps the requested craft first, even when a loosely related product has a better rating
{
    var a = new ProductSearchAnalysisDto { EnglishQuery = "jamdani", CategorySlug = "jamdani-weaving", Sort = "rating" };
    var jamdani = Make("Jamdani 4 star", 9000, 4.0m, 10, 5);
    var pot = Make("Pot 5 star", 500, 5.0m, 50, 5, "pottery-terracotta", "pot");
    var svc = new ProductSearchService(new FakeProvider(Ai("semantic", "relaxed", a, (jamdani, 1.0), (pot, 0.9))), new FakeRepo(new[] { jamdani, pot }));
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "bhalo jamdani" }, CancellationToken.None);
    Check("rating sort keeps the requested craft ahead of better-rated unrelated products", r.Items[0].Id == jamdani.Id);
}

// 5. filter-only answers come from the database with DB paging and the requested sort
{
    var a = new ProductSearchAnalysisDto { Semantic = false, Sort = "rating", CategorySlug = "jamdani-weaving", District = "Dhaka", InStockOnly = true, MaxPrice = 10000 };
    var repo = new FakeRepo(new[] { strong });
    var svc = new ProductSearchService(new FakeProvider(Ai("filter_only", "none", a)), repo);
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "highest rated jamdani", Page = 2, PageSize = 5 }, CancellationToken.None);
    Check("filter-only mode reported", r.Mode == "filter");
    Check("filter-only is paged in the database", repo.LastTake == 5 && repo.LastSkip == 5);
    Check("stock, price, district and sort reach PostgreSQL", repo.LastCriteria!.InStockOnly && repo.LastCriteria.MaxPrice == 10000 && repo.LastCriteria.District == "Dhaka" && repo.LastCriteria.Sort == "rating");
    Check("filter-only items carry no fake semantic score", r.Items[0].MatchScore is null);
}

// 6. AI service down -> keyword fallback, never an error
{
    var repo = new FakeRepo(new[] { strong });
    var svc = new ProductSearchService(new FakeProvider(null), repo);
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "jamdani khuje dao" }, CancellationToken.None);
    Check("fallback mode when the AI service is unavailable", r.Mode == "fallback" && r.Interpretation is null);
    Check("fallback strips filler words", repo.LastCriteria!.Keywords.SequenceEqual(new[] { "jamdani" }));
}

// 7. prices/stock in the response come from the database entity, not from the AI candidates
{
    var stale = Make("Stale", 5000, 4.0m, 3, 0);
    var a = new ProductSearchAnalysisDto { EnglishQuery = "stale" };
    var svc = new ProductSearchService(new FakeProvider(Ai("semantic", "strict", a, (stale, 1.0))), new FakeRepo(new[] { stale }));
    var r = await svc.SearchAsync(new ProductSearchQuery { Q = "stale" }, CancellationToken.None);
    Check("out-of-stock is reported from PostgreSQL data", !r.Items[0].InStock && r.Items[0].EffectivePrice == 5000);
}

Console.WriteLine(failures == 0 ? "ALL PASS" : $"{failures} FAILED");
return failures == 0 ? 0 : 1;

sealed class FakeProvider : IProductSearchCandidateProvider
{
    private readonly ProductSearchCandidatesDto? _result;
    public FakeProvider(ProductSearchCandidatesDto? result) => _result = result;
    public Task<ProductSearchCandidatesDto?> GetCandidatesAsync(string query, int limit, CancellationToken cancellationToken) => Task.FromResult(_result);
}

sealed class FakeRepo : IProductSearchQueryRepository
{
    private readonly IReadOnlyList<Product> _products;
    public ProductSearchCriteria? LastCriteria;
    public int? LastSkip, LastTake;
    public FakeRepo(IEnumerable<Product> products) => _products = products.ToList();

    public Task<(List<Product> Items, int Total)> SearchAsync(ProductSearchCriteria c, int? skip, int? take, CancellationToken cancellationToken)
    {
        LastCriteria = c; LastSkip = skip; LastTake = take;
        var items = c.Ids is null ? _products.ToList() : _products.Where(p => c.Ids.Contains(p.Id)).ToList();
        return Task.FromResult((items, items.Count));
    }
}
