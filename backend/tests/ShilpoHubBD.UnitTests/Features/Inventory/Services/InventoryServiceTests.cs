using ShilpoHubBD.Application.DTOs.Inventory;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Inventory;
using ShilpoHubBD.Domain.Entities.Inventory;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Inventory.Services;

[Trait("Feature", "Inventory")]
[Trait("Layer", "Service")]
public class InventoryServiceTests
{
    private readonly IInventoryRepository _inventory = Substitute.For<IInventoryRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly Guid _producerId = Guid.NewGuid();

    public InventoryServiceTests()
    {
        _users.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(TestUsers.Create(fullName: "Rahima Begum"));
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private InventoryService CreateService() => new(_inventory, _products, _users);

    private Product MakeProduct(int stock = 10, params ProductVariant[] variants)
    {
        var product = new Product { Id = Guid.NewGuid(), ProducerId = _producerId, Stock = stock };
        foreach (var v in variants)
        {
            product.Variants.Add(v);
        }

        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        return product;
    }

    private static AdjustStockRequest Request(int change, string reason = "Restock", Guid? variantId = null)
        => new() { ChangeAmount = change, Reason = reason, VariantId = variantId };

    // ---------- AdjustStockAsync ----------

    [Fact]
    public async Task AdjustStockAsync_PositiveChangeOnBaseProduct_IncreasesStockAndRecordsTheTransaction()
    {
        var product = MakeProduct(stock: 10);
        InventoryTransaction? saved = null;
        await _inventory.AddAsync(Arg.Do<InventoryTransaction>(t => saved = t), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var dto = await CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(5), Ct);

        Assert.Equal(15, product.Stock);
        Assert.InRange(product.UpdatedAt, before, DateTime.UtcNow);
        Assert.NotNull(saved);
        Assert.Equal(5, saved.ChangeAmount);
        Assert.Equal(10, saved.PreviousStock);
        Assert.Equal(15, saved.NewStock);
        Assert.Equal("Restock", saved.Reason);
        Assert.Equal(_producerId, saved.CreatedByUserId);
        Assert.InRange(saved.CreatedAt, before, DateTime.UtcNow);
        await _inventory.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(15, dto.NewStock);
        Assert.Equal("Rahima Begum", dto.CreatedByName);
        Assert.Null(dto.VariantId);
    }

    [Fact]
    public async Task AdjustStockAsync_NegativeChange_DecreasesStock()
    {
        var product = MakeProduct(stock: 10);

        var dto = await CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(-4), Ct);

        Assert.Equal(6, product.Stock);
        Assert.Equal(6, dto.NewStock);
    }

    [Fact]
    public async Task AdjustStockAsync_ReasonIsTrimmed()
    {
        var product = MakeProduct();
        InventoryTransaction? saved = null;
        await _inventory.AddAsync(Arg.Do<InventoryTransaction>(t => saved = t), Arg.Any<CancellationToken>());

        await CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(1, "  Damaged stock  "), Ct);

        Assert.Equal("Damaged stock", saved!.Reason);
    }

    [Fact]
    public async Task AdjustStockAsync_TargetsAVariant_AdjustsTheVariantNotTheBaseProduct()
    {
        var variant = new ProductVariant { Id = Guid.NewGuid(), Name = "Large", Stock = 3 };
        var product = MakeProduct(stock: 10, variant);
        InventoryTransaction? saved = null;
        await _inventory.AddAsync(Arg.Do<InventoryTransaction>(t => saved = t), Arg.Any<CancellationToken>());

        var dto = await CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(2, variantId: variant.Id), Ct);

        Assert.Equal(5, variant.Stock);
        Assert.Equal(10, product.Stock);
        Assert.Equal(variant.Id, saved!.ProductVariantId);
        Assert.Equal("Large", dto.VariantName);
        Assert.Equal(variant.Id, dto.VariantId);
    }

    [Fact]
    public async Task AdjustStockAsync_UnknownVariant_ThrowsNotFound()
    {
        var product = MakeProduct();

        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(1, variantId: Guid.NewGuid()), Ct));

        Assert.Equal("Variant not found.", error.Message);
    }

    [Fact]
    public async Task AdjustStockAsync_ResultingStockWouldGoNegative_ThrowsConflictAndLeavesStockUnchanged()
    {
        var product = MakeProduct(stock: 3);

        var error = await Assert.ThrowsAsync<ConflictException>(
            () => CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(-4), Ct));

        Assert.Equal("This adjustment would result in negative stock.", error.Message);
        Assert.Equal(3, product.Stock);
        await _inventory.DidNotReceive().AddAsync(Arg.Any<InventoryTransaction>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustStockAsync_ResultExactlyZero_Succeeds()
    {
        var product = MakeProduct(stock: 4);

        var dto = await CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(-4), Ct);

        Assert.Equal(0, dto.NewStock);
    }

    [Fact]
    public async Task AdjustStockAsync_UnknownProduct_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().AdjustStockAsync(Guid.NewGuid(), _producerId, false, Request(1), Ct));

        Assert.Equal("Product not found.", error.Message);
    }

    [Fact]
    public async Task AdjustStockAsync_AnotherProducersProductWithoutAdmin_ThrowsUnauthorized()
    {
        var product = MakeProduct();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().AdjustStockAsync(product.Id, Guid.NewGuid(), false, Request(1), Ct));

        Assert.Equal("You do not have permission to manage this product's inventory.", error.Message);
    }

    [Fact]
    public async Task AdjustStockAsync_AdminAdjustingAnotherProducersProduct_Succeeds()
    {
        var product = MakeProduct(stock: 5);

        var dto = await CreateService().AdjustStockAsync(product.Id, Guid.NewGuid(), true, Request(1), Ct);

        Assert.Equal(6, dto.NewStock);
    }

    [Fact]
    public async Task AdjustStockAsync_UnknownCurrentUser_ThrowsNotFound()
    {
        var product = MakeProduct();
        _users.GetByIdAsync(_producerId, Arg.Any<CancellationToken>()).Returns((ShilpoHubBD.Domain.Entities.Identity.User?)null);

        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().AdjustStockAsync(product.Id, _producerId, false, Request(1), Ct));

        Assert.Equal("User not found.", error.Message);
    }

    // ---------- GetHistoryAsync ----------

    [Fact]
    public async Task GetHistoryAsync_ReturnsMappedTransactions()
    {
        var product = MakeProduct();
        var transaction = new InventoryTransaction
        {
            Id = Guid.NewGuid(), ProductId = product.Id, ChangeAmount = 3, Reason = "x", PreviousStock = 5, NewStock = 8,
            CreatedBy = TestUsers.Create(fullName: "Admin Person"), CreatedAt = DateTime.UtcNow,
        };
        _inventory.GetByProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(new List<InventoryTransaction> { transaction });

        var result = await CreateService().GetHistoryAsync(product.Id, _producerId, false, Ct);

        var dto = Assert.Single(result);
        Assert.Equal(transaction.Id, dto.Id);
        Assert.Equal("Admin Person", dto.CreatedByName);
    }

    [Fact]
    public async Task GetHistoryAsync_UnknownProduct_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().GetHistoryAsync(Guid.NewGuid(), _producerId, false, Ct));

        Assert.Equal("Product not found.", error.Message);
    }

    [Fact]
    public async Task GetHistoryAsync_AnotherProducersProductWithoutAdmin_ThrowsUnauthorized()
    {
        var product = MakeProduct();

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().GetHistoryAsync(product.Id, Guid.NewGuid(), false, Ct));
    }

    [Fact]
    public async Task GetHistoryAsync_Admin_CanViewAnyProducersHistory()
    {
        var product = MakeProduct();
        _inventory.GetByProductAsync(product.Id, Arg.Any<CancellationToken>()).Returns(new List<InventoryTransaction>());

        var result = await CreateService().GetHistoryAsync(product.Id, Guid.NewGuid(), true, Ct);

        Assert.Empty(result);
    }

    // ---------- GetLowStockAsync ----------

    [Fact]
    public async Task GetLowStockAsync_MapsEachProductWithItsPrimaryImage()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Jamdani Saree", Slug = "jamdani-saree", Stock = 2, LowStockThreshold = 5,
        };
        product.Images.Add(new ProductImage { ImageUrl = "second.jpg", DisplayOrder = 2 });
        product.Images.Add(new ProductImage { ImageUrl = "first.jpg", DisplayOrder = 1 });
        _products.GetLowStockByProducerAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<Product> { product });

        var result = await CreateService().GetLowStockAsync(_producerId, Ct);

        var dto = Assert.Single(result);
        Assert.Equal("Jamdani Saree", dto.Name);
        Assert.Equal(2, dto.Stock);
        Assert.Equal(5, dto.LowStockThreshold);
        Assert.Equal("first.jpg", dto.PrimaryImageUrl);
    }

    [Fact]
    public async Task GetLowStockAsync_NoLowStockProducts_ReturnsEmpty()
    {
        _products.GetLowStockByProducerAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<Product>());

        Assert.Empty(await CreateService().GetLowStockAsync(_producerId, Ct));
    }
}
