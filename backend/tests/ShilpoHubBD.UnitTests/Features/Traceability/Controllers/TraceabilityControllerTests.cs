using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Controllers;

[Trait("Feature", "Traceability")]
[Trait("Layer", "Controller")]
public class TraceabilityControllerTests
{
    private readonly ITraceabilityService _service = Substitute.For<ITraceabilityService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TraceabilityController CreateController(params string[] roles) => new TraceabilityController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiTraceability()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(TraceabilityController)));
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(TraceabilityController)));
        Assert.Equal("api/traceability", AccessRules.ControllerRoute(typeof(TraceabilityController)));
    }

    [Fact]
    public void GetByProduct_HasNoRoleRestriction()
        => Assert.Empty(typeof(TraceabilityController).GetMethod(nameof(TraceabilityController.GetByProduct))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false));

    [Theory]
    [InlineData(nameof(TraceabilityController.Create))]
    [InlineData(nameof(TraceabilityController.Update))]
    [InlineData(nameof(TraceabilityController.Delete))]
    public void WriteActions_RestrictToProducerOrSuperAdmin(string action)
    {
        var attribute = typeof(TraceabilityController).GetMethod(action)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}", attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(TraceabilityController.GetByProduct), "GET", "products/{productId:guid}")]
    [InlineData(nameof(TraceabilityController.Create), "POST", null)]
    [InlineData(nameof(TraceabilityController.Update), "PUT", "{id:guid}")]
    [InlineData(nameof(TraceabilityController.Delete), "DELETE", "{id:guid}")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(TraceabilityController), action));

    [Fact]
    public async Task GetByProduct_ReturnsTheRecordForThatProduct()
    {
        var productId = Guid.NewGuid();
        var dto = new ProductTraceabilityDto { ProductId = productId };
        _service.GetByProductIdAsync(productId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetByProduct(productId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Create_UsesTheSignedInProducerAndReturns201PointingAtGetByProduct()
    {
        var request = new CreateProductTraceabilityRequest { ProductId = Guid.NewGuid() };
        var dto = new ProductTraceabilityDto { ProductId = request.ProductId };
        _service.CreateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Create(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TraceabilityController.GetByProduct), created.ActionName);
        Assert.Equal(request.ProductId, created.RouteValues?["productId"]);
        Assert.Same(dto, created.Value);
    }

    [Fact]
    public async Task Update_PassesTheSignedInUserAndWhetherTheyAreAdmin()
    {
        var id = Guid.NewGuid();
        var request = new UpdateProductTraceabilityRequest { Summary = "New" };
        var dto = new ProductTraceabilityDto { Id = id };
        _service.UpdateAsync(id, _userId, true, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).Update(id, request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Update_ProducerNotAdmin_PassesIsAdminFalse()
    {
        var id = Guid.NewGuid();
        var request = new UpdateProductTraceabilityRequest { Summary = "New" };

        await CreateController(RoleNames.Producer).Update(id, request, Ct);

        await _service.Received(1).UpdateAsync(id, _userId, false, request, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_PassesTheSignedInUserAndWhetherTheyAreAdmin()
    {
        var id = Guid.NewGuid();

        var result = await CreateController(RoleNames.SuperAdmin).Delete(id, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).DeleteAsync(id, _userId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Delete_Producer_PassesIsAdminFalse()
    {
        var id = Guid.NewGuid();

        await CreateController(RoleNames.Producer).Delete(id, Ct);

        await _service.Received(1).DeleteAsync(id, _userId, false, Arg.Any<CancellationToken>());
    }
}
