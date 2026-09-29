using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.ProducerBusiness;
using ShilpoHubBD.Application.DTOs.SupplierDiscovery;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.SupplierDiscovery.Controllers;

[Trait("Feature", "SupplierDiscovery")]
[Trait("Layer", "Controller")]
public class SupplierDiscoveryControllerTests
{
    private readonly ISupplierDiscoveryService _service = Substitute.For<ISupplierDiscoveryService>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private SupplierDiscoveryController CreateController() => new SupplierDiscoveryController(_service).WithUser(Guid.NewGuid(), RoleNames.BusinessPartner);

    [Fact]
    public void Controller_RestrictsToBusinessPartnerOrSuperAdmin()
    {
        Assert.Equal($"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}", AccessRules.ClassRoles(typeof(SupplierDiscoveryController)));
        Assert.Equal("api/supplier-discovery", AccessRules.ControllerRoute(typeof(SupplierDiscoveryController)));
    }

    [Theory]
    [InlineData(nameof(SupplierDiscoveryController.Search), "GET", "search")]
    [InlineData(nameof(SupplierDiscoveryController.GetProducerProfile), "GET", "producers/{producerId:guid}")]
    [InlineData(nameof(SupplierDiscoveryController.GetBusinessProfile), "GET", "producers/{producerId:guid}/business-profile")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(SupplierDiscoveryController), action));

    [Fact]
    public async Task Search_ReturnsTheServicesResult()
    {
        var parameters = new SupplierSearchParameters();
        var page = new PagedResult<SupplierSearchResultDto>();
        _service.SearchAsync(parameters, Arg.Any<CancellationToken>()).Returns(page);

        var result = await CreateController().Search(parameters, Ct);

        Assert.Same(page, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetProducerProfile_ReturnsTheServicesResult()
    {
        var producerId = Guid.NewGuid();
        var dto = new SupplierProfileDto { ProducerId = producerId };
        _service.GetProducerProfileAsync(producerId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetProducerProfile(producerId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetBusinessProfile_ReturnsTheServicesResult()
    {
        var producerId = Guid.NewGuid();
        var dto = new ProducerBusinessProfileDto { ProducerId = producerId };
        _service.GetBusinessProfileAsync(producerId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetBusinessProfile(producerId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
