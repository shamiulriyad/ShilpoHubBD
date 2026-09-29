using ShilpoHubBD.Application.DTOs.Marketplace;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;
using RecommendationService = ShilpoHubBD.Application.Services.Recommendation.RecommendationService;

namespace ShilpoHubBD.UnitTests.Features.Recommendation.Services;

[Trait("Feature", "Recommendation")]
[Trait("Layer", "Service")]
public class RecommendationServiceTests
{
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly IRecommendationProvider _provider = Substitute.For<IRecommendationProvider>();
    private readonly RecommendationService _service;

    public RecommendationServiceTests()
    {
        _service = new RecommendationService(_productRepository, _provider);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Product MakeProduct()
    {
        var producer = TestUsers.Create(fullName: "Producer");
        return new Product
        {
            Id = Guid.NewGuid(), Name = "Nakshi Kantha", Slug = "nakshi-kantha", Price = 500,
            Category = new Category { Id = Guid.NewGuid(), Name = "Textiles" }, CategoryId = Guid.NewGuid(),
            District = new District { Id = Guid.NewGuid(), Name = "Dhaka" }, DistrictId = Guid.NewGuid(),
            Producer = producer, ProducerId = producer.Id,
        };
    }

    [Fact]
    public async Task GetRecommendedForMeAsync_FetchesACandidatePoolAndDelegatesToTheProvider()
    {
        var product = MakeProduct();
        _productRepository.GetPagedAsync(Arg.Is<ProductQueryParameters>(q => q.Page == 1 && q.PageSize == 200), Arg.Any<CancellationToken>())
            .Returns((new List<Product> { product }, 1));
        _provider.RecommendForUserAsync(Arg.Any<Guid?>(), Arg.Any<List<Product>>(), 8, Arg.Any<CancellationToken>())
            .Returns(new List<Product> { product });

        var result = await _service.GetRecommendedForMeAsync(Guid.NewGuid(), 8, Ct);

        Assert.Equal(product.Id, Assert.Single(result).Id);
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(51, 50)]
    [InlineData(20, 20)]
    public async Task GetRecommendedForMeAsync_ClampsCountBetweenOneAndFifty(int requested, int expectedClamped)
    {
        _productRepository.GetPagedAsync(Arg.Any<ProductQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<Product>(), 0));
        _provider.RecommendForUserAsync(Arg.Any<Guid?>(), Arg.Any<List<Product>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product>());

        await _service.GetRecommendedForMeAsync(null, requested, Ct);

        await _provider.Received(1).RecommendForUserAsync(null, Arg.Any<List<Product>>(), expectedClamped, Ct);
    }

    [Fact]
    public async Task GetRecommendedForMeAsync_AnonymousCaller_PassesNullUserId()
    {
        _productRepository.GetPagedAsync(Arg.Any<ProductQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<Product>(), 0));
        _provider.RecommendForUserAsync(Arg.Any<Guid?>(), Arg.Any<List<Product>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product>());

        await _service.GetRecommendedForMeAsync(null, 8, Ct);

        await _provider.Received(1).RecommendForUserAsync(null, Arg.Any<List<Product>>(), 8, Ct);
    }

    [Fact]
    public async Task GetSimilarAsync_UnknownProduct_ThrowsNotFound()
    {
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetSimilarAsync(Guid.NewGuid(), 8, Ct));
    }

    [Fact]
    public async Task GetSimilarAsync_DelegatesToTheProviderWithTheCandidatePool()
    {
        var product = MakeProduct();
        var similar = MakeProduct();
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.GetPagedAsync(Arg.Any<ProductQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<Product> { similar }, 1));
        _provider.RecommendSimilarAsync(product, Arg.Any<List<Product>>(), 8, Arg.Any<CancellationToken>())
            .Returns(new List<Product> { similar });

        var result = await _service.GetSimilarAsync(product.Id, 8, Ct);

        Assert.Equal(similar.Id, Assert.Single(result).Id);
    }

    [Fact]
    public async Task GetSimilarAsync_MapsProductFieldsIncludingPrimaryImageAndNavigationNames()
    {
        var product = MakeProduct();
        var similar = MakeProduct();
        similar.Images.Add(new ProductImage { Id = Guid.NewGuid(), ImageUrl = "/img/2.png", DisplayOrder = 1 });
        similar.Images.Add(new ProductImage { Id = Guid.NewGuid(), ImageUrl = "/img/1.png", DisplayOrder = 0 });
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _productRepository.GetPagedAsync(Arg.Any<ProductQueryParameters>(), Arg.Any<CancellationToken>()).Returns((new List<Product> { similar }, 1));
        _provider.RecommendSimilarAsync(product, Arg.Any<List<Product>>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<Product> { similar });

        var result = await _service.GetSimilarAsync(product.Id, 8, Ct);

        var dto = Assert.Single(result);
        Assert.Equal("/img/1.png", dto.PrimaryImageUrl);
        Assert.Equal(similar.Category.Name, dto.CategoryName);
        Assert.Equal(similar.District.Name, dto.DistrictName);
        Assert.Equal(similar.Producer.FullName, dto.ProducerName);
    }
}
