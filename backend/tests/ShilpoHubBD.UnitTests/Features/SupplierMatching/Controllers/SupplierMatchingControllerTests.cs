using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.SupplierMatching;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.SupplierMatching.Controllers;

[Trait("Feature", "SupplierMatching")]
[Trait("Layer", "Controller")]
public class SupplierMatchingControllerTests
{
    private readonly ISupplierMatchingService _service = Substitute.For<ISupplierMatchingService>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Controller_RestrictsToBusinessPartnerOrSuperAdmin()
    {
        Assert.Equal($"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}", AccessRules.ClassRoles(typeof(SupplierMatchingController)));
        Assert.Equal("api/supplier-matching", AccessRules.ControllerRoute(typeof(SupplierMatchingController)));
    }

    [Fact]
    public void Match_UsesPostVerbAndItsRoute()
        => Assert.Equal(("POST", "match"), AccessRules.ActionRoute(typeof(SupplierMatchingController), nameof(SupplierMatchingController.Match)));

    [Fact]
    public async Task Match_ReturnsTheServicesResult()
    {
        var request = new SupplierMatchRequest();
        var list = new List<SupplierMatchResultDto> { new() { ProducerId = Guid.NewGuid() } };
        _service.MatchAsync(request, Arg.Any<CancellationToken>()).Returns(list);

        var result = await new SupplierMatchingController(_service).WithUser(Guid.NewGuid(), RoleNames.BusinessPartner).Match(request, Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
