using ShilpoHubBD.Application.DTOs.CounterfeitDetection;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.CounterfeitDetection;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.UnitTests.Features.CounterfeitDetection.Services;

[Trait("Feature", "CounterfeitDetection")]
[Trait("Layer", "Service")]
public class CounterfeitDetectionServiceTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly ICounterfeitDetectionProvider _provider = Substitute.For<ICounterfeitDetectionProvider>();
    private readonly CounterfeitDetectionService _service;

    public CounterfeitDetectionServiceTests()
    {
        _service = new CounterfeitDetectionService(_productRepository, _provider);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CheckAsync_UnknownProduct_ThrowsNotFound()
    {
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.CheckAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task CheckAsync_BuildsContextFromTheProductAndCategoryStats_ThenReturnsTheProvidersVerdict()
    {
        var categoryId = Guid.NewGuid();
        var product = new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Price = 500, CategoryId = categoryId,
            HandmadeVerificationStatus = HandmadeVerificationStatus.Verified, ApprovalStatus = ProductApprovalStatus.Approved,
            ReviewCount = 3, SalesCount = 10,
        };
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.GetCategoryPriceStatsAsync(categoryId, Arg.Any<CancellationToken>()).Returns((1000m, 20));
        _provider.Check(Arg.Is<CounterfeitCheckContext>(c =>
            c.ProductName == "Nakshi Kantha" && c.Price == 500 && c.CategoryAveragePrice == 1000m && c.CategorySampleSize == 20
            && c.HandmadeVerificationStatus == HandmadeVerificationStatus.Verified && c.ApprovalStatus == ProductApprovalStatus.Approved
            && c.ReviewCount == 3 && c.SalesCount == 10))
            .Returns((15m, "Low", new List<string> { "No counterfeit risk signals detected." }));

        var result = await _service.CheckAsync(product.Id, Ct);

        Assert.Equal(product.Id, result.ProductId);
        Assert.Equal("Nakshi Kantha", result.ProductName);
        Assert.Equal(15m, result.RiskScore);
        Assert.Equal("Low", result.RiskLevel);
        Assert.Equal("No counterfeit risk signals detected.", Assert.Single(result.Signals));
    }
}
