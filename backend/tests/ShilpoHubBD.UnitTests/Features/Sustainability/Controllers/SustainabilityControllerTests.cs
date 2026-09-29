using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Sustainability;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Sustainability.Controllers;

[Trait("Feature", "Sustainability")]
[Trait("Layer", "Controller")]
public class SustainabilityControllerTests
{
    private readonly ISustainabilityService _service = Substitute.For<ISustainabilityService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private SustainabilityController CreateController(params string[] roles) => new SustainabilityController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiSustainability()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(SustainabilityController)));
        Assert.False(AccessRules.ClassRequiresSignIn(typeof(SustainabilityController)));
        Assert.Equal("api/sustainability", AccessRules.ControllerRoute(typeof(SustainabilityController)));
    }

    [Fact]
    public void GetByProducer_HasNoRoleRestriction()
        => Assert.Empty(typeof(SustainabilityController).GetMethod(nameof(SustainabilityController.GetByProducer))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false));

    [Theory]
    [InlineData(nameof(SustainabilityController.GetMine))]
    [InlineData(nameof(SustainabilityController.AddMaterialRecord))]
    [InlineData(nameof(SustainabilityController.AddCertification))]
    public void ProducerActions_RestrictToProducer(string action)
    {
        var attribute = typeof(SustainabilityController).GetMethod(action)!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Equal(RoleNames.Producer, attribute.Roles);
    }

    [Fact]
    public void VerifyCertification_RestrictsToSuperAdmin()
    {
        var attribute = typeof(SustainabilityController).GetMethod(nameof(SustainabilityController.VerifyCertification))!
            .GetCustomAttributes(typeof(AuthorizeAttribute), false).Cast<AuthorizeAttribute>().Single();
        Assert.Equal(RoleNames.SuperAdmin, attribute.Roles);
    }

    [Theory]
    [InlineData(nameof(SustainabilityController.GetMine), "GET", "me")]
    [InlineData(nameof(SustainabilityController.GetByProducer), "GET", "producers/{producerId:guid}")]
    [InlineData(nameof(SustainabilityController.AddMaterialRecord), "POST", "materials")]
    [InlineData(nameof(SustainabilityController.AddCertification), "POST", "certifications")]
    [InlineData(nameof(SustainabilityController.VerifyCertification), "POST", "certifications/{certificationId:guid}/verify")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string? template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(SustainabilityController), action));

    [Fact]
    public async Task GetMine_ReturnsTheSignedInProducersProfile()
    {
        var dto = new SustainabilityProfileDto { ProducerId = _userId };
        _service.GetMyProfileAsync(_userId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).GetMine(Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetByProducer_ReturnsThatProducersProfile()
    {
        var producerId = Guid.NewGuid();
        var dto = new SustainabilityProfileDto { ProducerId = producerId };
        _service.GetByProducerIdAsync(producerId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await new SustainabilityController(_service).WithAnonymousRequest().GetByProducer(producerId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task AddMaterialRecord_UsesTheSignedInProducer()
    {
        var request = new CreateMaterialRecordRequest { MaterialName = "Cotton" };
        var dto = new SustainableMaterialRecordDto { Id = Guid.NewGuid() };
        _service.AddMaterialRecordAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).AddMaterialRecord(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task AddCertification_UsesTheSignedInProducer()
    {
        var request = new CreateMaterialCertificationRequest { MaterialName = "Cotton" };
        var dto = new SustainableMaterialCertificationDto { Id = Guid.NewGuid() };
        _service.AddCertificationAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).AddCertification(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task VerifyCertification_ReturnsTheVerifiedCertification()
    {
        var certificationId = Guid.NewGuid();
        var dto = new SustainableMaterialCertificationDto { Id = certificationId, IsVerified = true };
        _service.VerifyCertificationAsync(certificationId, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.SuperAdmin).VerifyCertification(certificationId, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
