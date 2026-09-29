using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Inventory;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Inventory.Controllers;

[Trait("Feature", "Inventory")]
[Trait("Layer", "Controller")]
public class InventoryControllerTests
{
    private readonly IInventoryService _service = Substitute.For<IInventoryService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private InventoryController CreateController(params string[] roles) => new InventoryController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_IsRestrictedToProducerAndSuperAdminUnderApiInventory()
    {
        Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}", AccessRules.ClassRoles(typeof(InventoryController)));
        Assert.Equal("api/inventory", AccessRules.ControllerRoute(typeof(InventoryController)));
    }

    [Theory]
    [InlineData(nameof(InventoryController.AdjustStock), "POST", "products/{productId:guid}/adjust")]
    [InlineData(nameof(InventoryController.GetHistory), "GET", "products/{productId:guid}/history")]
    [InlineData(nameof(InventoryController.GetLowStock), "GET", "low-stock")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(InventoryController), action));

    [Fact]
    public async Task AdjustStock_PassesTheSignedInUserAndWhetherTheyAreAdmin()
    {
        var productId = Guid.NewGuid();
        var request = new AdjustStockRequest { ChangeAmount = 5, Reason = "Restock" };
        var dto = new InventoryTransactionDto { ProductId = productId, NewStock = 15 };
        _service.AdjustStockAsync(productId, _userId, false, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).AdjustStock(productId, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task AdjustStock_SignedInAsSuperAdmin_PassesIsAdminTrue()
    {
        var productId = Guid.NewGuid();
        var request = new AdjustStockRequest { ChangeAmount = 1, Reason = "x" };

        await CreateController(RoleNames.SuperAdmin).AdjustStock(productId, request, Ct);

        await _service.Received(1).AdjustStockAsync(productId, _userId, true, request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AdjustStock_AnotherProducersProduct_PropagatesUnauthorized()
    {
        _service.AdjustStockAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<bool>(), Arg.Any<AdjustStockRequest>(), Arg.Any<CancellationToken>())
            .Returns<InventoryTransactionDto>(_ => throw new UnauthorizedAccessException("You do not have permission to manage this product's inventory."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateController(RoleNames.Producer).AdjustStock(Guid.NewGuid(), new AdjustStockRequest(), Ct));
    }

    [Fact]
    public async Task GetHistory_ReturnsTheProductsTransactionHistory()
    {
        var productId = Guid.NewGuid();
        var history = new List<InventoryTransactionDto> { new() { ProductId = productId } };
        _service.GetHistoryAsync(productId, _userId, false, Arg.Any<CancellationToken>()).Returns(history);

        var result = await CreateController(RoleNames.Producer).GetHistory(productId, Ct);

        Assert.Same(history, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetLowStock_NoProducerIdGiven_UsesTheSignedInUsersOwnId()
    {
        await CreateController(RoleNames.Producer).GetLowStock(null, Ct);

        await _service.Received(1).GetLowStockAsync(_userId, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLowStock_AdminWithAProducerId_LooksUpThatProducer()
    {
        var otherProducer = Guid.NewGuid();

        await CreateController(RoleNames.SuperAdmin).GetLowStock(otherProducer, Ct);

        await _service.Received(1).GetLowStockAsync(otherProducer, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GetLowStock_NonAdminWithAProducerIdQueryParameter_IgnoresItAndUsesTheirOwnId()
    {
        var otherProducer = Guid.NewGuid();

        await CreateController(RoleNames.Producer).GetLowStock(otherProducer, Ct);

        await _service.Received(1).GetLowStockAsync(_userId, Arg.Any<CancellationToken>());
    }
}
