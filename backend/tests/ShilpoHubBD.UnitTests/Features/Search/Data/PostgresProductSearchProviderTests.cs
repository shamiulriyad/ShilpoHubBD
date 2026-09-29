using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Search;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Search.Data;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Search")]
[Trait("Layer", "Data")]
[Trait("Needs", "Database")]
public class PostgresProductSearchProviderTests
{
    private readonly TestDatabaseFixture _database;

    public PostgresProductSearchProviderTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static async Task<(Category Category, District District, ShilpoHubBD.Domain.Entities.Identity.User Producer)> SeedReferenceAsync(ShilpoHubDbContext context)
    {
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var district = await context.Districts.FirstAsync(default);
        var producer = TestUsers.Create();
        context.Categories.Add(category);
        context.Users.Add(producer);
        return (category, district, producer);
    }

    private static Product MakeProduct(
        Category category, District district, ShilpoHubBD.Domain.Entities.Identity.User producer, string name, string description = "d",
        bool active = true, ProductApprovalStatus status = ProductApprovalStatus.Approved) => new()
    {
        Id = Guid.NewGuid(), Name = name, Slug = Guid.NewGuid().ToString("N"), Description = description, Price = 1000, Stock = 1,
        IsActive = active, ApprovalStatus = status, CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public void Name_IsPostgresFullTextSearch()
        => Assert.Equal("PostgresFullTextSearch", new PostgresProductSearchProvider(
            new ShilpoHubDbContext(new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<ShilpoHubDbContext>()
                .UseNpgsql("Host=localhost;Database=x;Username=x;Password=x").Options)).Name);

    [Fact]
    public async Task SearchAsync_MatchesOnProductName()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district, producer) = await SeedReferenceAsync(context);
        var tag = Guid.NewGuid().ToString("N")[..8];
        context.Products.Add(MakeProduct(category, district, producer, $"Jamdani Saree {tag}"));
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 1, 12, Ct);

        Assert.Equal(1, total);
        Assert.Contains(items, p => p.Name.Contains(tag, StringComparison.Ordinal));
    }

    [Fact]
    public async Task SearchAsync_MatchesOnDescriptionViaSubstring()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district, producer) = await SeedReferenceAsync(context);
        var tag = Guid.NewGuid().ToString("N")[..8];
        context.Products.Add(MakeProduct(category, district, producer, "Some Product", description: $"Handwoven, tag {tag}"));
        await context.SaveChangesAsync(Ct);

        var (items, total) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 1, 12, Ct);

        Assert.Equal(1, total);
        Assert.Contains(items, p => p.Description.Contains(tag, StringComparison.Ordinal));
    }

    [Fact]
    public async Task SearchAsync_InactiveProduct_IsExcluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district, producer) = await SeedReferenceAsync(context);
        var tag = Guid.NewGuid().ToString("N")[..8];
        context.Products.Add(MakeProduct(category, district, producer, $"Inactive {tag}", active: false));
        await context.SaveChangesAsync(Ct);

        var (_, total) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 1, 12, Ct);

        Assert.Equal(0, total);
    }

    [Fact]
    public async Task SearchAsync_PendingApproval_IsExcluded()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district, producer) = await SeedReferenceAsync(context);
        var tag = Guid.NewGuid().ToString("N")[..8];
        context.Products.Add(MakeProduct(category, district, producer, $"Pending {tag}", status: ProductApprovalStatus.Pending));
        await context.SaveChangesAsync(Ct);

        var (_, total) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 1, 12, Ct);

        Assert.Equal(0, total);
    }

    [Fact]
    public async Task SearchAsync_NoMatch_ReturnsEmptyWithZeroTotal()
    {
        await using var db = await _database.BeginAsync();

        var (items, total) = await new PostgresProductSearchProvider(db.NewContext())
            .SearchAsync(Guid.NewGuid().ToString("N"), 1, 12, Ct);

        Assert.Empty(items);
        Assert.Equal(0, total);
    }

    [Fact]
    public async Task SearchAsync_Paging_ReturnsTheRequestedSliceWithTheFullTotal()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district, producer) = await SeedReferenceAsync(context);
        var tag = Guid.NewGuid().ToString("N")[..8];
        context.Products.AddRange(
            MakeProduct(category, district, producer, $"Item One {tag}"),
            MakeProduct(category, district, producer, $"Item Two {tag}"),
            MakeProduct(category, district, producer, $"Item Three {tag}"));
        await context.SaveChangesAsync(Ct);

        var (page1, total) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 1, 2, Ct);
        var (page2, _) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 2, 2, Ct);

        Assert.Equal(3, total);
        Assert.Equal(2, page1.Count);
        Assert.Single(page2);
    }

    [Fact]
    public async Task SearchAsync_LoadsCategoryDistrictProducerAndImages()
    {
        await using var db = await _database.BeginAsync();
        var context = db.NewContext();
        var (category, district, producer) = await SeedReferenceAsync(context);
        var tag = Guid.NewGuid().ToString("N")[..8];
        var product = MakeProduct(category, district, producer, $"Loaded {tag}");
        context.Products.Add(product);
        await context.SaveChangesAsync(Ct);
        var imageContext = db.NewContext();
        imageContext.ProductImages.Add(new ProductImage { Id = Guid.NewGuid(), ProductId = product.Id, ImageUrl = "a.jpg", DisplayOrder = 0 });
        await imageContext.SaveChangesAsync(Ct);

        var (items, _) = await new PostgresProductSearchProvider(db.NewContext()).SearchAsync(tag, 1, 12, Ct);

        var found = Assert.Single(items);
        Assert.Equal(category.Id, found.Category.Id);
        Assert.Equal(district.Id, found.District.Id);
        Assert.Equal(producer.Id, found.Producer.Id);
        Assert.Single(found.Images);
    }
}
