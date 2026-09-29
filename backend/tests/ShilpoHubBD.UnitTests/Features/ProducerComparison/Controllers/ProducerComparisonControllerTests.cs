using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.ProducerComparison;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.ProducerComparison.Controllers;

[Trait("Feature", "ProducerComparison")]
[Trait("Layer", "Controller")]
public class ProducerComparisonControllerTests
{
    private readonly IProducerComparisonService _service = Substitute.For<IProducerComparisonService>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void Controller_RestrictsToBusinessPartnerOrSuperAdmin()
    {
        Assert.Equal($"{RoleNames.BusinessPartner},{RoleNames.SuperAdmin}", AccessRules.ClassRoles(typeof(ProducerComparisonController)));
        Assert.Equal("api/producer-comparison", AccessRules.ControllerRoute(typeof(ProducerComparisonController)));
    }

    [Fact]
    public void Compare_UsesPostVerbAndItsRoute()
        => Assert.Equal(("POST", "compare"), AccessRules.ActionRoute(typeof(ProducerComparisonController), nameof(ProducerComparisonController.Compare)));

    [Fact]
    public async Task Compare_ReturnsTheServicesResult()
    {
        var request = new ProducerComparisonRequest { ProducerIds = [Guid.NewGuid(), Guid.NewGuid()] };
        var rows = new List<ProducerComparisonRowDto> { new() { ProducerId = Guid.NewGuid() } };
        _service.CompareAsync(request, Arg.Any<CancellationToken>()).Returns(rows);

        var result = await new ProducerComparisonController(_service).WithUser(Guid.NewGuid(), RoleNames.BusinessPartner).Compare(request, Ct);

        Assert.Same(rows, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
