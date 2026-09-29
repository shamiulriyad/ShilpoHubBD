using System.Text;
using Microsoft.AspNetCore.Mvc;
using ShilpoHubBD.Api.Controllers;
using ShilpoHubBD.Application.DTOs.Certificate;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Constants;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Certificate.Controllers;

[Trait("Feature", "Certificate")]
[Trait("Layer", "Controller")]
public class CertificatesControllerTests
{
    private readonly ICertificateService _service = Substitute.For<ICertificateService>();
    private readonly Guid _userId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private CertificatesController CreateController(params string[] roles) => new CertificatesController(_service).WithUser(_userId, roles);

    [Fact]
    public void Controller_HasNoClassLevelRoleRestrictionUnderApiCertificates()
    {
        Assert.Null(AccessRules.ClassRoles(typeof(CertificatesController)));
        Assert.Equal("api/certificates", AccessRules.ControllerRoute(typeof(CertificatesController)));
    }

    [Theory]
    [InlineData(nameof(CertificatesController.GetById))]
    [InlineData(nameof(CertificatesController.Download))]
    [InlineData(nameof(CertificatesController.Verify))]
    public void PublicActions_HaveNoRoleRestriction(string action)
    {
        var attribute = typeof(CertificatesController).GetMethod(action)!
            .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false);
        Assert.Empty(attribute);
    }

    [Theory]
    [InlineData(nameof(CertificatesController.GetMine))]
    [InlineData(nameof(CertificatesController.Generate))]
    [InlineData(nameof(CertificatesController.Revoke))]
    public void ProducerActions_AreRestrictedToProducerAndSuperAdmin(string action)
        => Assert.Equal($"{RoleNames.Producer},{RoleNames.SuperAdmin}",
            typeof(CertificatesController).GetMethod(action)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>().Single().Roles);

    [Theory]
    [InlineData(nameof(CertificatesController.GetById), "GET", "{id:guid}")]
    [InlineData(nameof(CertificatesController.Download), "GET", "{id:guid}/download")]
    [InlineData(nameof(CertificatesController.GetMine), "GET", "mine")]
    [InlineData(nameof(CertificatesController.Generate), "POST", "generate")]
    [InlineData(nameof(CertificatesController.Revoke), "POST", "{id:guid}/revoke")]
    [InlineData(nameof(CertificatesController.Verify), "POST", "verify")]
    public void Actions_UseTheirVerbAndRoute(string action, string method, string template)
        => Assert.Equal((method, template), AccessRules.ActionRoute(typeof(CertificatesController), action));

    [Fact]
    public async Task GetById_ReturnsTheCertificate()
    {
        var dto = new CertificateDto { Id = Guid.NewGuid() };
        _service.GetByIdAsync(dto.Id, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().GetById(dto.Id, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task GetById_Unknown_PropagatesNotFound()
    {
        _service.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns<CertificateDto>(_ => throw new NotFoundException("Certificate not found."));

        await Assert.ThrowsAsync<NotFoundException>(() => CreateController().GetById(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task Download_ReturnsTheHtmlFileWithItsFileName()
    {
        var id = Guid.NewGuid();
        _service.GetDownloadAsync(id, Arg.Any<CancellationToken>()).Returns(("Certificate-SH-1.html", "<html></html>"));

        var result = await CreateController().Download(id, Ct);

        var file = Assert.IsType<FileContentResult>(result);
        Assert.Equal("Certificate-SH-1.html", file.FileDownloadName);
        Assert.Equal("text/html", file.ContentType);
        Assert.Equal(Encoding.UTF8.GetBytes("<html></html>"), file.FileContents);
    }

    [Fact]
    public async Task GetMine_ReturnsTheSignedInProducersCertificates()
    {
        var certificates = new List<CertificateDto> { new() };
        _service.GetMineAsync(_userId, Arg.Any<CancellationToken>()).Returns(certificates);

        var result = await CreateController(RoleNames.Producer).GetMine(Ct);

        Assert.Same(certificates, Assert.IsType<OkObjectResult>(result.Result).Value);
    }

    [Fact]
    public async Task Generate_UsesTheSignedInProducerAndReturns201PointingAtGetById()
    {
        var request = new GenerateCertificateRequest { ProductId = Guid.NewGuid() };
        var dto = new CertificateDto { Id = Guid.NewGuid() };
        _service.GenerateAsync(_userId, request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController(RoleNames.Producer).Generate(request, Ct);

        var created = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(CertificatesController.GetById), created.ActionName);
        Assert.Equal(dto.Id, created.RouteValues?["id"]);
        Assert.Same(dto, created.Value);
    }

    [Fact]
    public async Task Generate_AnotherProducersProduct_PropagatesUnauthorized()
    {
        _service.GenerateAsync(Arg.Any<Guid>(), Arg.Any<GenerateCertificateRequest>(), Arg.Any<CancellationToken>())
            .Returns<CertificateDto>(_ => throw new UnauthorizedAccessException("You can only generate certificates for your own products."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateController(RoleNames.Producer).Generate(new GenerateCertificateRequest(), Ct));
    }

    [Fact]
    public async Task Revoke_UsesTheSignedInUserAndReturnsNoContent()
    {
        var id = Guid.NewGuid();

        var result = await CreateController(RoleNames.Producer).Revoke(id, Ct);

        Assert.IsType<NoContentResult>(result);
        await _service.Received(1).RevokeAsync(id, _userId, false, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Revoke_SignedInAsSuperAdmin_PassesIsAdminTrue()
    {
        var id = Guid.NewGuid();

        await CreateController(RoleNames.SuperAdmin).Revoke(id, Ct);

        await _service.Received(1).RevokeAsync(id, _userId, true, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Verify_ReturnsTheVerificationResult()
    {
        var request = new VerifyCertificateRequest { CertificateNumber = "SH-1" };
        var dto = new CertificateVerificationResultDto { IsValid = true, CertificateNumber = "SH-1" };
        _service.VerifyAsync(request, Arg.Any<CancellationToken>()).Returns(dto);

        var result = await CreateController().Verify(request, Ct);

        Assert.Same(dto, Assert.IsType<OkObjectResult>(result.Result).Value);
    }
}
