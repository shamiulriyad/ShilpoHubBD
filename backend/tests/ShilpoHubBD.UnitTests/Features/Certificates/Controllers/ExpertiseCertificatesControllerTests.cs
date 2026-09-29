using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Certificates;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Certificates.Controllers;

[Trait("Feature", "Certificates")]
[Trait("Layer", "Controller")]
public class ExpertiseCertificatesControllerTests
{
    private readonly IExpertiseCertificateService _service = Substitute.For<IExpertiseCertificateService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private ExpertiseCertificatesController CreateController(params string[] roles)
        => new ExpertiseCertificatesController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionOrRoutePrefix()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(ExpertiseCertificatesController)));
        Assert.Null(AccessRules.ControllerRoute(typeof(ExpertiseCertificatesController)));
    }

    [Theory]
    [InlineData(nameof(ExpertiseCertificatesController.Mine), $"{RoleNames.Producer},{RoleNames.SuperAdmin}")]
    [InlineData(nameof(ExpertiseCertificatesController.Eligible), RoleNames.SuperAdmin)]
    [InlineData(nameof(ExpertiseCertificatesController.Issue), RoleNames.SuperAdmin)]
    public void Actions_AreRestrictedToTheExpectedRoles(string action, string expectedRoles)
        => Assert.Equal(expectedRoles,
            typeof(ExpertiseCertificatesController).GetMethod(action)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles);

    [Theory]
    [InlineData(nameof(ExpertiseCertificatesController.Mine), "GET", "api/expertise-certificates/mine")]
    [InlineData(nameof(ExpertiseCertificatesController.Eligible), "GET", "api/admin/expertise-certificates/eligible")]
    [InlineData(nameof(ExpertiseCertificatesController.Issue), "POST", "api/admin/expertise-certificates/issue")]
    public void Actions_UseTheirVerbAndFullRoute(string action, string method, string template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(ExpertiseCertificatesController), action));

    [Fact]
    public async Task Mine_ReturnsTheSignedInProducersProgress()
    {
        var dto = new ExpertiseProgressDto { RatingCount = 10 };
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Mine(Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Eligible_ReturnsTheEligibleProducerList()
    {
        var list = new List<EligibleProducerDto> { new() { ProducerName = "Rahima Begum" } };
        _service.GetEligibleAsync(Arg.Any<CancellationToken>()).Returns(list);

        var result = await CreateController(RoleNames.SuperAdmin).Eligible(Ct);

        Assert.Same(list, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Issue_UsesTheSignedInAdminAsIssuer()
    {
        var producerId = Guid.NewGuid();
        var dto = new ExpertiseCertificateDto { ProducerId = producerId, Level = "Bronze" };
        _service.IssueAsync(producerId, _userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).Issue(new IssueExpertiseCertificateRequest { ProducerId = producerId }, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Issue_ProducerNotYetEligible_PropagatesTheConflict()
    {
        _service.IssueAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<ExpertiseCertificateDto>(_ => throw new ConflictException("This producer has no customer ratings yet."));

        await Assert.ThrowsAsync<ConflictException>(
            () => CreateController(RoleNames.SuperAdmin).Issue(new IssueExpertiseCertificateRequest(), Ct));
    }
}
