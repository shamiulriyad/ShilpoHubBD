// Regression checks for the real Fashion Matching feature (FashionMatchingService): different inputs
// produce different results, an unavailable AI attribute-extraction service still falls back to a
// real (not fake) keyword-based match, a genuinely-understood item with no complementary stock
// returns an honest empty list rather than an invented substitute, an unrecognised description falls
// back to a clearly-labelled generic pick, and every single returned item name traces back to a real
// product in the fixture's catalog -- never a hardcoded string. No database -- in-memory fakes only.
// Run with `dotnet run`.
using Microsoft.Extensions.Logging.Abstractions;
using ShilpoHubBD.Application.DTOs.AIShopping;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.AIShopping;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

var failures = 0;
void Check(string name, bool ok) { Console.WriteLine((ok ? "PASS " : "FAIL ") + name); if (!ok) failures++; }

var producer = new User { Id = Guid.NewGuid(), FullName = "Test Producer", CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
var district = new District { Id = Guid.NewGuid(), Name = "Dhaka", Division = "Dhaka" };

var jamdani = new Category { Id = Guid.NewGuid(), Name = "Dhakai Jamdani", Slug = "jamdani-weaving" };
var handloom = new Category { Id = Guid.NewGuid(), Name = "Handloom Textiles", Slug = "handloom-textiles" };
var jewellery = new Category { Id = Guid.NewGuid(), Name = "Jewellery & Metalwork", Slug = "jewellery-metalwork" };
var brass = new Category { Id = Guid.NewGuid(), Name = "Brass & Bell Metal", Slug = "brass-bell-metal" };
var leather = new Category { Id = Guid.NewGuid(), Name = "Leather Craft", Slug = "leather-craft" };
var allCategories = new List<Category> { jamdani, handloom, jewellery, brass, leather };

Product MakeProduct(string name, Category category, decimal price, int sales, List<string>? colors = null, List<string>? occasions = null)
{
    var product = new Product
    {
        Id = Guid.NewGuid(),
        Name = name,
        Slug = name.ToLowerInvariant().Replace(' ', '-'),
        Description = name,
        Price = price,
        CategoryId = category.Id,
        Category = category,
        DistrictId = district.Id,
        District = district,
        ProducerId = producer.Id,
        Producer = producer,
        SalesCount = sales,
    };
    if (colors is not null || occasions is not null)
    {
        product.Attributes = new ProductAttributes
        {
            ProductId = product.Id,
            Product = product,
            Colors = colors ?? new List<string>(),
            Occasions = occasions ?? new List<string>(),
        };
    }

    return product;
}

var pSaree = MakeProduct("Red Silk Jamdani Saree", jamdani, 3000m, sales: 50);
var pEarrings = MakeProduct("Gold Filigree Earrings", jewellery, 1200m, sales: 120, colors: new() { "Gold" }, occasions: new() { "Wedding" });
var pAnklet = MakeProduct("Silver Anklet Set", jewellery, 800m, sales: 30);
var pClutch = MakeProduct("Leather Clutch Bag", leather, 1500m, sales: 40);
var pShawl = MakeProduct("Handwoven Cotton Shawl", handloom, 900m, sales: 60, occasions: new() { "Wedding" });
var pBangle = MakeProduct("Brass Bangle Set", brass, 600m, sales: 25, colors: new() { "Gold" });
var allProducts = new List<Product> { pSaree, pEarrings, pAnklet, pClutch, pShawl, pBangle };
var allNames = allProducts.Select(p => p.Name).ToHashSet();

ProductSearchCandidatesDto Analysis(
    string? categorySlug = null, List<string>? colors = null, List<string>? occasions = null, decimal? maxPrice = null) => new()
{
    Analysis = new ProductSearchAnalysisDto
    {
        CategorySlug = categorySlug,
        Colors = colors ?? new(),
        Occasions = occasions ?? new(),
        MaxPrice = maxPrice,
    },
    Mode = "semantic",
    Pass = "strict",
};

FashionMatchingService MakeService(ProductSearchCandidatesDto? analysis, List<Product> products, List<Category> categories) => new(
    new FakeCandidateProvider(analysis),
    new FakeProductSearchQueryRepository(products),
    new FakeCategoryRepository(categories),
    NullLogger<FashionMatchingService>.Instance);

// ===================== Input A: a Jamdani (textile) item, gold-colored request =====================
var serviceA = MakeService(Analysis(categorySlug: "jamdani-weaving", colors: new() { "Gold" }), allProducts, allCategories);
var resultA = await serviceA.GetMatchesAsync(
    new FashionMatchRequest { ItemDescription = "Looking for gold accessories to go with my jamdani saree" }, CancellationToken.None);
var namesA = resultA.Select(r => r.ItemName).ToHashSet();

Check("Jamdani input: returns only complementary (jewellery/bag) family products",
    namesA.SetEquals(new[] { "Gold Filigree Earrings", "Silver Anklet Set", "Leather Clutch Bag", "Brass Bangle Set" }));
Check("Jamdani input: never suggests the same textile family as the described item",
    !namesA.Contains("Red Silk Jamdani Saree") && !namesA.Contains("Handwoven Cotton Shawl"));
Check("Jamdani input: a real gold-colored product is ranked first (real attribute-match bonus, not random)",
    resultA.Count > 0 && (resultA[0].ItemName == "Gold Filigree Earrings" || resultA[0].ItemName == "Brass Bangle Set"));

// ===================== Input B: a jewellery item -- different input must change the results =====================
var serviceB = MakeService(Analysis(categorySlug: "jewellery-metalwork"), allProducts, allCategories);
var resultB = await serviceB.GetMatchesAsync(new FashionMatchRequest { ItemDescription = "gold earrings" }, CancellationToken.None);
var namesB = resultB.Select(r => r.ItemName).ToHashSet();

Check("Jewellery input: returns complementary textile products instead", namesB.SetEquals(new[] { "Red Silk Jamdani Saree", "Handwoven Cotton Shawl" }));
Check("Different input produces different results (input must affect results)", !namesA.SetEquals(namesB));

// ===================== AI attribute-extraction unavailable: real keyword fallback, not fake/empty =====================
var serviceDown = MakeService(null, allProducts, allCategories);
var resultDown = await serviceDown.GetMatchesAsync(new FashionMatchRequest { ItemDescription = "A beautiful Jamdani piece" }, CancellationToken.None);
var namesDown = resultDown.Select(r => r.ItemName).ToHashSet();
Check("AI service unavailable: falls back to real keyword category matching over the real catalog",
    namesDown.SetEquals(new[] { "Gold Filigree Earrings", "Silver Anklet Set", "Leather Clutch Bag", "Brass Bangle Set" }));

// ===================== Honest no-match: category understood, but zero real complementary stock =====================
var jewelleryOnlyCategories = new List<Category> { jewellery };
var jewelleryOnlyProducts = new List<Product> { pEarrings, pAnklet };
var serviceNoStock = MakeService(Analysis(categorySlug: "jewellery-metalwork"), jewelleryOnlyProducts, jewelleryOnlyCategories);
var resultNoStock = await serviceNoStock.GetMatchesAsync(new FashionMatchRequest { ItemDescription = "gold earrings" }, CancellationToken.None);
Check("Honest no-match: an understood category with no real complementary stock returns an empty list, never an invented substitute",
    resultNoStock.Count == 0);

// ===================== Unrecognised input: honest, clearly-labelled fallback pool =====================
var resultUnrecognised = await serviceDown.GetMatchesAsync(new FashionMatchRequest { ItemDescription = "asdkjaskjd nonsense text" }, CancellationToken.None);
Check("Unrecognised input: still returns real products from the catalog, never invented",
    resultUnrecognised.Count > 0 && resultUnrecognised.All(r => allNames.Contains(r.ItemName)));
Check("Unrecognised input: each suggestion is honestly labelled as a generic pick, not a targeted match",
    resultUnrecognised.All(r => r.Reason.Contains("couldn't identify")));

// ===================== Empty description: still real output, never an error or fake data =====================
var resultEmpty = await serviceDown.GetMatchesAsync(new FashionMatchRequest { ItemDescription = "" }, CancellationToken.None);
Check("Empty description: still returns real fallback products rather than erroring or faking", resultEmpty.Count > 0);

// ===================== Price range extracted from the description is honoured =====================
var serviceBudget = MakeService(Analysis(categorySlug: "jamdani-weaving", maxPrice: 1000m), allProducts, allCategories);
var resultBudget = await serviceBudget.GetMatchesAsync(
    new FashionMatchRequest { ItemDescription = "something affordable to match my jamdani, under 1000 taka" }, CancellationToken.None);
var namesBudget = resultBudget.Select(r => r.ItemName).ToHashSet();
Check("Price range from the description excludes products above budget (Leather Clutch Bag = 1500)", !namesBudget.Contains("Leather Clutch Bag"));
Check("Price range from the description still includes in-budget products", namesBudget.Contains("Silver Anklet Set") && namesBudget.Contains("Brass Bangle Set"));

// ===================== No hardcoded products: every result across every scenario is a real catalog item =====================
Check("Every result across all scenarios corresponds to a real product in the fixture's catalog, never a hardcoded/invented one",
    resultA.All(r => allNames.Contains(r.ItemName)) && resultB.All(r => allNames.Contains(r.ItemName)) && resultDown.All(r => allNames.Contains(r.ItemName)));

// ===================== ProductId: real, non-empty, and traceable to the actual matched product record =====================
var idByName = allProducts.ToDictionary(p => p.Name, p => p.Id);
var allResults = resultA.Concat(resultB).Concat(resultDown).Concat(resultNoStock).Concat(resultUnrecognised).Concat(resultEmpty).Concat(resultBudget).ToList();
Check("Every result carries a non-empty ProductId (never a default/invented Guid)",
    allResults.All(r => r.ProductId != Guid.Empty));
Check("Every result's ProductId is the real database id of the product named in ItemName, not a mismatched or random one",
    allResults.All(r => r.ProductId == idByName[r.ItemName]));

Console.WriteLine();
Console.WriteLine(failures == 0 ? "ALL PASSED" : $"{failures} FAILURE(S)");
return failures == 0 ? 0 : 1;

class FakeCategoryRepository : ICategoryRepository
{
    private readonly List<Category> _categories;
    public FakeCategoryRepository(List<Category> categories) => _categories = categories;

    public Task<List<Category>> GetAllAsync(bool includeInactive, CancellationToken cancellationToken) => Task.FromResult(_categories);
    public Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken) => Task.FromResult(_categories.FirstOrDefault(c => c.Id == id));
    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken) => Task.FromResult(_categories.Any(c => c.Slug == slug));
    public Task<bool> HasProductsAsync(Guid categoryId, CancellationToken cancellationToken) => Task.FromResult(false);
    public Task<Dictionary<Guid, int>> GetActiveProductCountsAsync(CancellationToken cancellationToken) => Task.FromResult(new Dictionary<Guid, int>());
    public Task AddAsync(Category category, CancellationToken cancellationToken) => Task.CompletedTask;
    public void Remove(Category category) { }
    public Task SaveChangesAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// Mirrors the real ProductSearchQueryRepository's contract (category/price filtering over real rows);
// the production implementation additionally restricts to IsActive+Approved, enforced in PostgreSQL --
// not re-tested here since that is the repository's own guarantee, not FashionMatchingService's.
class FakeProductSearchQueryRepository : IProductSearchQueryRepository
{
    private readonly List<Product> _products;
    public FakeProductSearchQueryRepository(List<Product> products) => _products = products;

    public Task<(List<Product> Items, int Total)> SearchAsync(ProductSearchCriteria c, int? skip, int? take, CancellationToken cancellationToken)
    {
        IEnumerable<Product> query = _products;
        if (!string.IsNullOrWhiteSpace(c.CategorySlug)) query = query.Where(p => p.Category.Slug == c.CategorySlug);
        if (c.MinPrice is { } min) query = query.Where(p => p.Price >= min);
        if (c.MaxPrice is { } max) query = query.Where(p => p.Price <= max);
        var list = query.ToList();
        return Task.FromResult((list, list.Count));
    }
}

class FakeCandidateProvider : IProductSearchCandidateProvider
{
    private readonly ProductSearchCandidatesDto? _response;
    public FakeCandidateProvider(ProductSearchCandidatesDto? response) => _response = response;
    public Task<ProductSearchCandidatesDto?> GetCandidatesAsync(string query, int limit, CancellationToken cancellationToken) => Task.FromResult(_response);
}
