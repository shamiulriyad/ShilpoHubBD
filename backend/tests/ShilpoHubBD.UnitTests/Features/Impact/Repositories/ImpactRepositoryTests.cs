using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Impact.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Impact")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class ImpactRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public ImpactRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Seeds a delivered (or other-status) order with one item for a customer, in a fresh category/producer.
    /// Picks the <paramref name="districtIndex"/>-th seeded district so callers can force distinct districts.
    /// Creates a fresh customer unless <paramref name="existingCustomerId"/> (from an earlier call) is given. Returns the customer's id.</summary>
    private static async Task<Guid> SeedOrderAsync(ShilpoHubDbContext context, OrderStatus status, int quantity = 1, int districtIndex = 0, Guid? existingCustomerId = null)
    {
        var district = await context.Districts.OrderBy(d => d.Id).Skip(districtIndex).FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var producer = TestUsers.Create();
        context.Users.Add(producer);

        Guid customerId;
        if (existingCustomerId is { } id)
        {
            customerId = id;
        }
        else
        {
            var customer = TestUsers.Create();
            context.Users.Add(customer);
            customerId = customer.Id;
        }

        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = Guid.NewGuid().ToString("N"), Price = 500, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var order = new Order
        {
            Id = Guid.NewGuid(), OrderNumber = "ORD-" + Guid.NewGuid().ToString("N")[..10], UserId = customerId,
            Status = status, PaymentMethod = PaymentMethod.CashOnDelivery, Subtotal = 500, Total = 500,
            RecipientName = "Customer", RecipientPhone = "0100000000", ShippingAddressLine = "House 1",
            ShippingDistrictId = district.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var item = new OrderItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, ProductId = product.Id, ProductName = product.Name,
            UnitPrice = 500, Quantity = quantity, LineTotal = 500 * quantity,
        };

        context.Categories.Add(category);
        context.Products.Add(product);
        context.Orders.Add(order);
        context.OrderItems.Add(item);
        await context.SaveChangesAsync(Ct);
        return customerId;
    }

    [Fact]
    public async Task GetImpactStatsAsync_NoOrders_ReturnsAllZeroes()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new ImpactRepository(scope.NewContext());

        var stats = await repository.GetImpactStatsAsync(Guid.NewGuid(), Ct);

        Assert.Equal(0, stats.FamiliesSupported);
        Assert.Equal(0, stats.DistinctDistrictsSupported);
        Assert.Equal(0, stats.DistinctCategoriesSupported);
        Assert.Equal(0, stats.TotalItemsPurchased);
    }

    [Theory]
    [InlineData(OrderStatus.Delivered)]
    [InlineData(OrderStatus.ReturnRequested)]
    [InlineData(OrderStatus.Returned)]
    [InlineData(OrderStatus.Refunded)]
    public async Task GetImpactStatsAsync_CompletedStatus_CountsTheOrder(OrderStatus status)
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var customerId = await SeedOrderAsync(seedContext, status, quantity: 2);

        var repository = new ImpactRepository(scope.NewContext());
        var stats = await repository.GetImpactStatsAsync(customerId, Ct);

        Assert.Equal(1, stats.FamiliesSupported);
        Assert.Equal(2, stats.TotalItemsPurchased);
    }

    [Theory]
    [InlineData(OrderStatus.Pending)]
    [InlineData(OrderStatus.Processing)]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Cancelled)]
    public async Task GetImpactStatsAsync_NotYetCompletedStatus_IgnoresTheOrder(OrderStatus status)
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var customerId = await SeedOrderAsync(seedContext, status);

        var repository = new ImpactRepository(scope.NewContext());
        var stats = await repository.GetImpactStatsAsync(customerId, Ct);

        Assert.Equal(0, stats.TotalItemsPurchased);
    }

    [Fact]
    public async Task GetImpactStatsAsync_OnlyCountsTheGivenCustomersOrders()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var customerId = await SeedOrderAsync(seedContext, OrderStatus.Delivered);
        await SeedOrderAsync(seedContext, OrderStatus.Delivered);

        var repository = new ImpactRepository(scope.NewContext());
        var stats = await repository.GetImpactStatsAsync(customerId, Ct);

        Assert.Equal(1, stats.TotalItemsPurchased);
    }

    [Fact]
    public async Task GetImpactStatsAsync_MultipleOrdersFromDifferentProducersAndDistricts_CountsDistinctBreadth()
    {
        await using var scope = await _database.BeginAsync();
        var customerId = await SeedOrderAsync(scope.NewContext(), OrderStatus.Delivered, districtIndex: 0);
        await SeedOrderAsync(scope.NewContext(), OrderStatus.Delivered, districtIndex: 1, existingCustomerId: customerId);

        var repository = new ImpactRepository(scope.NewContext());
        var stats = await repository.GetImpactStatsAsync(customerId, Ct);

        Assert.Equal(2, stats.FamiliesSupported);
        Assert.Equal(2, stats.DistinctDistrictsSupported);
        Assert.Equal(2, stats.DistinctCategoriesSupported);
        Assert.Equal(2, stats.TotalItemsPurchased);
    }
}
