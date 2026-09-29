using ShilpoHubBD.Application.DTOs.Certificate;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Services.Certificate;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using CertificateEntity = ShilpoHubBD.Domain.Entities.Certificate.Certificate;

namespace ShilpoHubBD.UnitTests.Features.Certificate.Services;

[Trait("Feature", "Certificate")]
[Trait("Layer", "Service")]
public class CertificateServiceTests
{
    private readonly ICertificateRepository _certificates = Substitute.For<ICertificateRepository>();
    private readonly IProductRepository _products = Substitute.For<IProductRepository>();
    private readonly Guid _producerId = Guid.NewGuid();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private CertificateService CreateService() => new(_certificates, _products);

    private Product MakeProduct()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Jamdani Saree", ProducerId = _producerId,
            Producer = TestUsers.Create(fullName: "Rahima Begum"),
            District = new District { Name = "Dhaka" }, Category = new Category { Name = "Weaving" },
        };
        _products.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        return product;
    }

    private static CertificateEntity MakeCertificate(Product product, bool revoked = false, string number = "SH-20260101-ABCD1234") => new()
    {
        Id = Guid.NewGuid(), ProductId = product.Id, ProducerId = product.ProducerId, CertificateNumber = number,
        ProductName = product.Name, ProducerName = product.Producer.FullName, District = product.District.Name,
        Category = product.Category.Name, IsRevoked = revoked, IssuedAt = DateTime.UtcNow,
    };

    // ---------- GenerateAsync ----------

    [Fact]
    public async Task GenerateAsync_NoExistingCertificate_IssuesANewOneSnapshottingCurrentProductDetails()
    {
        var product = MakeProduct();
        CertificateEntity? saved = null;
        await _certificates.AddAsync(Arg.Do<CertificateEntity>(c => saved = c), Arg.Any<CancellationToken>());
        var before = DateTime.UtcNow;

        var dto = await CreateService().GenerateAsync(_producerId, new GenerateCertificateRequest { ProductId = product.Id }, Ct);

        Assert.NotNull(saved);
        Assert.Equal(product.Id, saved.ProductId);
        Assert.Equal(_producerId, saved.ProducerId);
        Assert.Equal("Jamdani Saree", saved.ProductName);
        Assert.Equal("Rahima Begum", saved.ProducerName);
        Assert.Equal("Dhaka", saved.District);
        Assert.Equal("Weaving", saved.Category);
        Assert.False(saved.IsRevoked);
        Assert.InRange(saved.IssuedAt, before, DateTime.UtcNow);
        Assert.Matches(@"^SH-\d{8}-[0-9A-F]{8}$", saved.CertificateNumber);
        await _certificates.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        Assert.Equal(saved.CertificateNumber, dto.CertificateNumber);
    }

    [Fact]
    public async Task GenerateAsync_ActiveCertificateAlreadyExists_ReturnsItWithoutCreatingAnother()
    {
        var product = MakeProduct();
        var existing = MakeCertificate(product);
        _certificates.GetActiveByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var dto = await CreateService().GenerateAsync(_producerId, new GenerateCertificateRequest { ProductId = product.Id }, Ct);

        Assert.Equal(existing.Id, dto.Id);
        await _certificates.DidNotReceive().AddAsync(Arg.Any<CertificateEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateAsync_UnknownProduct_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(
            () => CreateService().GenerateAsync(_producerId, new GenerateCertificateRequest { ProductId = Guid.NewGuid() }, Ct));

        Assert.Equal("Product not found.", error.Message);
    }

    [Fact]
    public async Task GenerateAsync_AnotherProducersProduct_ThrowsUnauthorizedAndSavesNothing()
    {
        var product = MakeProduct();

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().GenerateAsync(Guid.NewGuid(), new GenerateCertificateRequest { ProductId = product.Id }, Ct));

        Assert.Equal("You can only generate certificates for your own products.", error.Message);
        await _certificates.DidNotReceive().AddAsync(Arg.Any<CertificateEntity>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task GenerateAsync_TwoCertificates_GetDifferentNumbers()
    {
        var productA = MakeProduct();
        var productB = MakeProduct();
        var service = CreateService();

        var first = await service.GenerateAsync(_producerId, new GenerateCertificateRequest { ProductId = productA.Id }, Ct);
        var second = await service.GenerateAsync(_producerId, new GenerateCertificateRequest { ProductId = productB.Id }, Ct);

        Assert.NotEqual(first.CertificateNumber, second.CertificateNumber);
    }

    // ---------- GetByIdAsync ----------

    [Fact]
    public async Task GetByIdAsync_ExistingCertificate_ReturnsIt()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        var dto = await CreateService().GetByIdAsync(certificate.Id, Ct);

        Assert.Equal(certificate.CertificateNumber, dto.CertificateNumber);
    }

    [Fact]
    public async Task GetByIdAsync_UnknownCertificate_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetByIdAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Certificate not found.", error.Message);
    }

    // ---------- GetMineAsync ----------

    [Fact]
    public async Task GetMineAsync_ReturnsTheProducersCertificates()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByProducerAsync(_producerId, Arg.Any<CancellationToken>()).Returns(new List<CertificateEntity> { certificate });

        var result = await CreateService().GetMineAsync(_producerId, Ct);

        Assert.Equal(certificate.Id, Assert.Single(result).Id);
    }

    // ---------- RevokeAsync ----------

    [Fact]
    public async Task RevokeAsync_ActiveCertificateOwnedByTheProducer_RevokesIt()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);
        var before = DateTime.UtcNow;

        await CreateService().RevokeAsync(certificate.Id, _producerId, false, Ct);

        Assert.True(certificate.IsRevoked);
        Assert.InRange(certificate.RevokedAt!.Value, before, DateTime.UtcNow);
        await _certificates.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeAsync_Admin_CanRevokeAnyProducersCertificate()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        await CreateService().RevokeAsync(certificate.Id, Guid.NewGuid(), true, Ct);

        Assert.True(certificate.IsRevoked);
    }

    [Fact]
    public async Task RevokeAsync_AnotherProducersCertificateWithoutAdmin_ThrowsUnauthorized()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        var error = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => CreateService().RevokeAsync(certificate.Id, Guid.NewGuid(), false, Ct));

        Assert.Equal("You do not have permission to manage this certificate.", error.Message);
        Assert.False(certificate.IsRevoked);
    }

    [Fact]
    public async Task RevokeAsync_AlreadyRevoked_ThrowsConflictAndSavesNothing()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product, revoked: true);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        var error = await Assert.ThrowsAsync<ConflictException>(() => CreateService().RevokeAsync(certificate.Id, _producerId, false, Ct));

        Assert.Equal("This certificate has already been revoked.", error.Message);
        await _certificates.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RevokeAsync_UnknownCertificate_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().RevokeAsync(Guid.NewGuid(), _producerId, false, Ct));

        Assert.Equal("Certificate not found.", error.Message);
    }

    // ---------- VerifyAsync ----------

    [Fact]
    public async Task VerifyAsync_ValidCertificate_ReturnsAuthenticWithDetails()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByCertificateNumberAsync(certificate.CertificateNumber, Arg.Any<CancellationToken>()).Returns(certificate);

        var result = await CreateService().VerifyAsync(new VerifyCertificateRequest { CertificateNumber = certificate.CertificateNumber }, Ct);

        Assert.True(result.IsValid);
        Assert.Equal("This certificate is authentic.", result.Message);
        Assert.Equal(certificate.ProductName, result.ProductName);
    }

    [Fact]
    public async Task VerifyAsync_TrimsTheCertificateNumberBeforeLookingItUp()
    {
        _certificates.GetByCertificateNumberAsync("SH-1", Arg.Any<CancellationToken>()).Returns((CertificateEntity?)null);

        await CreateService().VerifyAsync(new VerifyCertificateRequest { CertificateNumber = "  SH-1  " }, Ct);

        await _certificates.Received(1).GetByCertificateNumberAsync("SH-1", Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task VerifyAsync_RevokedCertificate_ReturnsInvalidWithADetailedMessageButStillTheDetails()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product, revoked: true);
        _certificates.GetByCertificateNumberAsync(certificate.CertificateNumber, Arg.Any<CancellationToken>()).Returns(certificate);

        var result = await CreateService().VerifyAsync(new VerifyCertificateRequest { CertificateNumber = certificate.CertificateNumber }, Ct);

        Assert.False(result.IsValid);
        Assert.Equal("This certificate has been revoked and is no longer valid.", result.Message);
        Assert.Equal(certificate.ProductName, result.ProductName);
    }

    [Fact]
    public async Task VerifyAsync_UnknownNumber_ReturnsInvalidWithNoDetails()
    {
        _certificates.GetByCertificateNumberAsync("SH-BOGUS", Arg.Any<CancellationToken>()).Returns((CertificateEntity?)null);

        var result = await CreateService().VerifyAsync(new VerifyCertificateRequest { CertificateNumber = "SH-BOGUS" }, Ct);

        Assert.False(result.IsValid);
        Assert.Equal("SH-BOGUS", result.CertificateNumber);
        Assert.Null(result.ProductName);
        Assert.Equal("No certificate was found with this number.", result.Message);
    }

    // ---------- GetDownloadAsync ----------

    [Fact]
    public async Task GetDownloadAsync_ExistingCertificate_ReturnsAFileNameAndHtmlContainingTheCertificateDetails()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        var (fileName, html) = await CreateService().GetDownloadAsync(certificate.Id, Ct);

        Assert.Equal($"Certificate-{certificate.CertificateNumber}.html", fileName);
        Assert.Contains(certificate.CertificateNumber, html);
        Assert.Contains(certificate.ProductName, html);
        Assert.Contains("AUTHENTIC", html);
    }

    [Fact]
    public async Task GetDownloadAsync_RevokedCertificate_HtmlShowsRevoked()
    {
        var product = MakeProduct();
        var certificate = MakeCertificate(product, revoked: true);
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        var (_, html) = await CreateService().GetDownloadAsync(certificate.Id, Ct);

        Assert.Contains("REVOKED", html);
    }

    [Fact]
    public async Task GetDownloadAsync_ProductNameWithHtmlCharacters_IsEscaped()
    {
        var product = MakeProduct();
        product.Name = "Saree <script>alert(1)</script>";
        var certificate = MakeCertificate(product);
        certificate.ProductName = product.Name;
        _certificates.GetByIdAsync(certificate.Id, Arg.Any<CancellationToken>()).Returns(certificate);

        var (_, html) = await CreateService().GetDownloadAsync(certificate.Id, Ct);

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }

    [Fact]
    public async Task GetDownloadAsync_UnknownCertificate_ThrowsNotFound()
    {
        var error = await Assert.ThrowsAsync<NotFoundException>(() => CreateService().GetDownloadAsync(Guid.NewGuid(), Ct));

        Assert.Equal("Certificate not found.", error.Message);
    }
}
