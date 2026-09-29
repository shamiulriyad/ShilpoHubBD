using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Data;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using ShilpoHubBD.UnitTests.Common.Database;

namespace ShilpoHubBD.UnitTests.Features.Complaints.Repositories;

[Collection(DatabaseCollection.Name)]
[Trait("Feature", "Complaints")]
[Trait("Layer", "Repository")]
[Trait("Needs", "Database")]
public class OrderComplaintRepositoryTests
{
    private readonly TestDatabaseFixture _database;

    public OrderComplaintRepositoryTests(TestDatabaseFixture database) => _database = database;

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Seeds a producer, product and delivered order (with one item) that a complaint can point at.</summary>
    private static async Task<(Order Order, Product Product, ShilpoHubBD.Domain.Entities.Identity.User Customer, ShilpoHubBD.Domain.Entities.Identity.User Producer)>
        SeedOrderAsync(ShilpoHubDbContext context)
    {
        var district = await context.Districts.FirstAsync(Ct);
        var category = new Category { Id = Guid.NewGuid(), Name = "Test Category " + Guid.NewGuid().ToString("N")[..8], Slug = Guid.NewGuid().ToString("N") };
        var producer = TestUsers.Create();
        var customer = TestUsers.Create();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = Guid.NewGuid().ToString("N"), Price = 500, Stock = 1,
            CategoryId = category.Id, DistrictId = district.Id, ProducerId = producer.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var order = new Order
        {
            Id = Guid.NewGuid(), OrderNumber = "ORD-" + Guid.NewGuid().ToString("N")[..10], UserId = customer.Id,
            Status = OrderStatus.Delivered, PaymentMethod = PaymentMethod.CashOnDelivery, Subtotal = 500, Total = 500,
            RecipientName = "Customer", RecipientPhone = "0100000000", ShippingAddressLine = "House 1",
            ShippingDistrictId = district.Id, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
        };
        var item = new OrderItem
        {
            Id = Guid.NewGuid(), OrderId = order.Id, ProductId = product.Id, ProductName = product.Name,
            UnitPrice = 500, Quantity = 1, LineTotal = 500,
        };

        context.Categories.Add(category);
        context.Users.AddRange(producer, customer);
        context.Products.Add(product);
        context.Orders.Add(order);
        context.OrderItems.Add(item);
        await context.SaveChangesAsync(Ct);
        return (order, product, customer, producer);
    }

    private static OrderComplaint MakeComplaint(Order order, Product product, Guid customerId, Guid producerId, OrderComplaintStatus status = OrderComplaintStatus.Open) => new()
    {
        Id = Guid.NewGuid(), OrderId = order.Id, ProductId = product.Id, ProducerId = producerId, CustomerId = customerId,
        Subject = "Broken item", Description = "It arrived cracked.", Status = status,
        CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow,
    };

    [Fact]
    public async Task GetByIdAsync_UnknownId_ReturnsNull()
    {
        await using var scope = await _database.BeginAsync();
        var repository = new OrderComplaintRepository(scope.NewContext());

        Assert.Null(await repository.GetByIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByIdAsync_KnownId_ReturnsItWithAllNavigationsLoaded()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (order, product, customer, producer) = await SeedOrderAsync(seedContext);
        var complaint = MakeComplaint(order, product, customer.Id, producer.Id);
        seedContext.OrderComplaints.Add(complaint);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new OrderComplaintRepository(scope.NewContext());
        var found = await repository.GetByIdAsync(complaint.Id, Ct);

        Assert.NotNull(found);
        Assert.Equal(order.OrderNumber, found!.Order.OrderNumber);
        Assert.Equal(product.Name, found.Product.Name);
        Assert.Equal(producer.Id, found.Producer.Id);
        Assert.Equal(customer.Id, found.Customer.Id);
    }

    [Fact]
    public async Task GetByCustomerAsync_ReturnsOnlyThatCustomersComplaints_NewestFirst()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (order, product, customer, producer) = await SeedOrderAsync(seedContext);
        var (otherOrder, otherProduct, otherCustomer, otherProducer) = await SeedOrderAsync(seedContext);

        var older = MakeComplaint(order, product, customer.Id, producer.Id);
        older.CreatedAt = DateTime.UtcNow.AddDays(-1);
        var newer = MakeComplaint(order, product, customer.Id, producer.Id);
        newer.CreatedAt = DateTime.UtcNow;
        var somebodyElses = MakeComplaint(otherOrder, otherProduct, otherCustomer.Id, otherProducer.Id);

        seedContext.OrderComplaints.AddRange(older, newer, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new OrderComplaintRepository(scope.NewContext());
        var result = await repository.GetByCustomerAsync(customer.Id, Ct);

        Assert.Equal(new[] { newer.Id, older.Id }, result.Select(c => c.Id));
    }

    [Fact]
    public async Task GetByProducerAsync_ReturnsOnlyThatProducersComplaints()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (order, product, customer, producer) = await SeedOrderAsync(seedContext);
        var (otherOrder, otherProduct, otherCustomer, otherProducer) = await SeedOrderAsync(seedContext);

        var mine = MakeComplaint(order, product, customer.Id, producer.Id);
        var somebodyElses = MakeComplaint(otherOrder, otherProduct, otherCustomer.Id, otherProducer.Id);
        seedContext.OrderComplaints.AddRange(mine, somebodyElses);
        await seedContext.SaveChangesAsync(Ct);

        var repository = new OrderComplaintRepository(scope.NewContext());
        var result = await repository.GetByProducerAsync(producer.Id, Ct);

        Assert.Equal(mine.Id, Assert.Single(result).Id);
    }

    [Theory]
    [InlineData(OrderComplaintStatus.Open, true)]
    [InlineData(OrderComplaintStatus.Resolved, true)]
    [InlineData(OrderComplaintStatus.Satisfied, false)]
    [InlineData(OrderComplaintStatus.Withdrawn, false)]
    public async Task HasUnsettledAsync_ReflectsWhetherTheComplaintIsStillOpenOrResolved(OrderComplaintStatus status, bool expected)
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (order, product, customer, producer) = await SeedOrderAsync(seedContext);
        seedContext.OrderComplaints.Add(MakeComplaint(order, product, customer.Id, producer.Id, status));
        await seedContext.SaveChangesAsync(Ct);

        var repository = new OrderComplaintRepository(scope.NewContext());
        Assert.Equal(expected, await repository.HasUnsettledAsync(customer.Id, product.Id, Ct));
    }

    [Theory]
    [InlineData(OrderComplaintStatus.Open, true)]
    [InlineData(OrderComplaintStatus.Resolved, true)]
    [InlineData(OrderComplaintStatus.Satisfied, false)]
    [InlineData(OrderComplaintStatus.Withdrawn, false)]
    public async Task HasOpenForOrderProductAsync_ReflectsWhetherThatOrderItemHasAnUnsettledComplaint(OrderComplaintStatus status, bool expected)
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (order, product, customer, producer) = await SeedOrderAsync(seedContext);
        seedContext.OrderComplaints.Add(MakeComplaint(order, product, customer.Id, producer.Id, status));
        await seedContext.SaveChangesAsync(Ct);

        var repository = new OrderComplaintRepository(scope.NewContext());
        Assert.Equal(expected, await repository.HasOpenForOrderProductAsync(order.Id, product.Id, Ct));
    }

    [Fact]
    public async Task AddAsync_ThenSaveChangesAsync_PersistsTheComplaint()
    {
        await using var scope = await _database.BeginAsync();
        var seedContext = scope.NewContext();
        var (order, product, customer, producer) = await SeedOrderAsync(seedContext);

        var repository = new OrderComplaintRepository(scope.NewContext());
        var complaint = MakeComplaint(order, product, customer.Id, producer.Id);
        await repository.AddAsync(complaint, Ct);
        await repository.SaveChangesAsync(Ct);

        var reloaded = await new OrderComplaintRepository(scope.NewContext()).GetByIdAsync(complaint.Id, Ct);
        Assert.NotNull(reloaded);
        Assert.Equal("Broken item", reloaded!.Subject);
    }
}
