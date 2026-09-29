using ShilpoHubBD.Application.DTOs.QRVerification;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.QRVerification;
using ShilpoHubBD.UnitTests.Common;
using QRVerificationService = ShilpoHubBD.Application.Services.QRVerification.QRVerificationService;

namespace ShilpoHubBD.UnitTests.Features.QRVerification.Services;

[Trait("Feature", "QRVerification")]
[Trait("Layer", "Service")]
public class QRVerificationServiceTests
{
    private readonly IQRVerificationRepository _repository = Substitute.For<IQRVerificationRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly QRVerificationService _service;

    public QRVerificationServiceTests()
    {
        _service = new QRVerificationService(_repository, _productRepository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Product MakeProduct(Guid producerId, string name = "Nakshi Kantha")
    {
        var producer = TestUsers.Create(fullName: "Producer");
        producer.Id = producerId;
        return new Product
        {
            Id = Guid.NewGuid(), Name = name, ProducerId = producerId, Producer = producer,
            District = new District { Id = Guid.NewGuid(), Name = "Dhaka" },
        };
    }

    [Fact]
    public async Task GenerateAsync_UnknownProduct_ThrowsNotFound()
    {
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GenerateAsync(Guid.NewGuid(), new GenerateQRCodeRequest { ProductId = Guid.NewGuid() }, Ct));
    }

    [Fact]
    public async Task GenerateAsync_NotYourProduct_ThrowsUnauthorized()
    {
        var product = MakeProduct(Guid.NewGuid());
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GenerateAsync(Guid.NewGuid(), new GenerateQRCodeRequest { ProductId = product.Id }, Ct));
    }

    [Fact]
    public async Task GenerateAsync_ActiveCodeAlreadyExists_ReturnsItWithoutCreatingANewOne()
    {
        var producerId = Guid.NewGuid();
        var product = MakeProduct(producerId);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        var existing = new QRCode { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Code = "abc", IsActive = true };
        _repository.GetActiveByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(existing);

        var result = await _service.GenerateAsync(producerId, new GenerateQRCodeRequest { ProductId = product.Id }, Ct);

        Assert.Equal(existing.Id, result.Id);
        await _repository.DidNotReceive().AddQRCodeAsync(Arg.Any<QRCode>(), Ct);
    }

    [Fact]
    public async Task GenerateAsync_NoActiveCode_CreatesAndSavesANewOne()
    {
        var producerId = Guid.NewGuid();
        var product = MakeProduct(producerId);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetActiveByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns((QRCode?)null);

        var result = await _service.GenerateAsync(producerId, new GenerateQRCodeRequest { ProductId = product.Id }, Ct);

        await _repository.Received(1).AddQRCodeAsync(Arg.Is<QRCode>(q => q.ProductId == product.Id && q.IsActive), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
        Assert.True(result.IsActive);
        Assert.Equal(product.Name, result.ProductName);
    }

    [Fact]
    public async Task RevokeAsync_UnknownCode_ThrowsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((QRCode?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.RevokeAsync(Guid.NewGuid(), Guid.NewGuid(), false, Ct));
    }

    [Fact]
    public async Task RevokeAsync_NotYourProductAndNotAdmin_ThrowsUnauthorized()
    {
        var qrCode = new QRCode { Id = Guid.NewGuid(), Product = MakeProduct(Guid.NewGuid()), IsActive = true };
        _repository.GetByIdAsync(qrCode.Id, Arg.Any<CancellationToken>()).Returns(qrCode);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.RevokeAsync(qrCode.Id, Guid.NewGuid(), false, Ct));
    }

    [Fact]
    public async Task RevokeAsync_AlreadyRevoked_ThrowsConflict()
    {
        var producerId = Guid.NewGuid();
        var qrCode = new QRCode { Id = Guid.NewGuid(), Product = MakeProduct(producerId), IsActive = false };
        _repository.GetByIdAsync(qrCode.Id, Arg.Any<CancellationToken>()).Returns(qrCode);

        await Assert.ThrowsAsync<ConflictException>(() => _service.RevokeAsync(qrCode.Id, producerId, false, Ct));
    }

    [Fact]
    public async Task RevokeAsync_Valid_SetsInactiveAndSaves()
    {
        var producerId = Guid.NewGuid();
        var qrCode = new QRCode { Id = Guid.NewGuid(), Product = MakeProduct(producerId), IsActive = true };
        _repository.GetByIdAsync(qrCode.Id, Arg.Any<CancellationToken>()).Returns(qrCode);

        await _service.RevokeAsync(qrCode.Id, producerId, false, Ct);

        Assert.False(qrCode.IsActive);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task RevokeAsync_Admin_CanRevokeSomeoneElsesCode()
    {
        var qrCode = new QRCode { Id = Guid.NewGuid(), Product = MakeProduct(Guid.NewGuid()), IsActive = true };
        _repository.GetByIdAsync(qrCode.Id, Arg.Any<CancellationToken>()).Returns(qrCode);

        await _service.RevokeAsync(qrCode.Id, Guid.NewGuid(), true, Ct);

        Assert.False(qrCode.IsActive);
    }

    [Fact]
    public async Task VerifyAsync_UnknownCode_RecordsAttemptAndReturnsInvalid()
    {
        _repository.GetByCodeAsync("missing", Arg.Any<CancellationToken>()).Returns((QRCode?)null);

        var result = await _service.VerifyAsync(Guid.NewGuid(), new VerifyQRRequest { Code = "  missing  " }, Ct);

        Assert.False(result.IsValid);
        Assert.Null(result.ProductId);
        await _repository.Received(1).AddVerificationRecordAsync(Arg.Is<QRVerificationRecord>(r =>
            r.ScannedCode == "missing" && r.QRCodeId == null && !r.IsValid), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task VerifyAsync_RevokedCode_ReturnsInvalidWithProductInfo()
    {
        var product = MakeProduct(Guid.NewGuid());
        var qrCode = new QRCode { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Code = "abc", IsActive = false };
        _repository.GetByCodeAsync("abc", Arg.Any<CancellationToken>()).Returns(qrCode);

        var result = await _service.VerifyAsync(null, new VerifyQRRequest { Code = "abc" }, Ct);

        Assert.False(result.IsValid);
        Assert.Equal(product.Id, result.ProductId);
        Assert.Equal(product.Name, result.ProductName);
    }

    [Fact]
    public async Task VerifyAsync_ActiveCode_ReturnsValidWithFullDetails()
    {
        var product = MakeProduct(Guid.NewGuid());
        var qrCode = new QRCode { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Code = "abc", IsActive = true };
        _repository.GetByCodeAsync("abc", Arg.Any<CancellationToken>()).Returns(qrCode);

        var result = await _service.VerifyAsync(Guid.NewGuid(), new VerifyQRRequest { Code = "abc" }, Ct);

        Assert.True(result.IsValid);
        Assert.Equal(product.Name, result.ProductName);
        Assert.Equal("Producer", result.ProducerName);
        Assert.Equal("Dhaka", result.District);
    }

    [Fact]
    public async Task VerifyAsync_AnonymousCaller_RecordsNullVerifiedByUserId()
    {
        var product = MakeProduct(Guid.NewGuid());
        var qrCode = new QRCode { Id = Guid.NewGuid(), ProductId = product.Id, Product = product, Code = "abc", IsActive = true };
        _repository.GetByCodeAsync("abc", Arg.Any<CancellationToken>()).Returns(qrCode);

        await _service.VerifyAsync(null, new VerifyQRRequest { Code = "abc" }, Ct);

        await _repository.Received(1).AddVerificationRecordAsync(Arg.Is<QRVerificationRecord>(r => r.VerifiedByUserId == null), Ct);
    }

    [Fact]
    public async Task GetMyHistoryAsync_ReturnsMappedPagedResult()
    {
        var userId = Guid.NewGuid();
        var record = new QRVerificationRecord { Id = Guid.NewGuid(), ScannedCode = "abc", IsValid = true, VerifiedAt = DateTime.UtcNow };
        _repository.GetHistoryForUserAsync(userId, 1, 20, Arg.Any<CancellationToken>()).Returns((new List<QRVerificationRecord> { record }, 1));

        var result = await _service.GetMyHistoryAsync(userId, new QRVerificationQueryParameters(), Ct);

        Assert.Equal(1, result.TotalCount);
        Assert.Equal("abc", Assert.Single(result.Items).ScannedCode);
    }

    [Fact]
    public async Task GetProductHistoryAsync_UnknownProduct_ThrowsNotFound()
    {
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.GetProductHistoryAsync(Guid.NewGuid(), Guid.NewGuid(), false, new QRVerificationQueryParameters(), Ct));
    }

    [Fact]
    public async Task GetProductHistoryAsync_NotYourProductAndNotAdmin_ThrowsUnauthorized()
    {
        var product = MakeProduct(Guid.NewGuid());
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.GetProductHistoryAsync(product.Id, Guid.NewGuid(), false, new QRVerificationQueryParameters(), Ct));
    }

    [Fact]
    public async Task GetProductHistoryAsync_Valid_ReturnsMappedPagedResult()
    {
        var producerId = Guid.NewGuid();
        var product = MakeProduct(producerId);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.GetHistoryForProductAsync(product.Id, 1, 20, Arg.Any<CancellationToken>()).Returns((new List<QRVerificationRecord>(), 0));

        var result = await _service.GetProductHistoryAsync(product.Id, producerId, false, new QRVerificationQueryParameters(), Ct);

        Assert.Empty(result.Items);
    }
}
