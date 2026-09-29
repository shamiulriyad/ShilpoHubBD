using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Search;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.UnitTests.Common;

namespace ShilpoHubBD.UnitTests.Features.Search.Services;

[Trait("Feature", "Search")]
[Trait("Layer", "Service")]
public class SearchServiceTests
{
    private readonly ISearchProvider _provider = Substitute.For<ISearchProvider>();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private SearchService CreateService() => new(_provider);

    private static Product MakeProduct(string name = "Jamdani Saree") => new()
    {
        Id = Guid.NewGuid(), Name = name, Slug = "jamdani-saree", Price = 5000, AverageRating = 4.5m, ReviewCount = 10,
        Category = new Category { Name = "Weaving" }, District = new District { Name = "Dhaka" },
        Producer = TestUsers.Create(fullName: "Rahima Begum"),
    };

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchAsync_EmptyOrWhitespaceQuery_ReturnsAnEmptyResultWithoutCallingTheProvider(string? query)
    {
        var result = await CreateService().SearchAsync(query!, 1, 12, Ct);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
        await _provider.DidNotReceive().SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_TrimsTheQueryBeforePassingItToTheProvider()
    {
        _provider.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((new List<Product>(), 0));

        await CreateService().SearchAsync("  jamdani  ", 1, 12, Ct);

        await _provider.Received(1).SearchAsync("jamdani", 1, 12, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SearchAsync_PassesThroughThePageAndPageSizeUnchanged()
    {
        _provider.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((new List<Product>(), 0));

        var result = await CreateService().SearchAsync("saree", 3, 24, Ct);

        await _provider.Received(1).SearchAsync("saree", 3, 24, Arg.Any<CancellationToken>());
        Assert.Equal(3, result.Page);
        Assert.Equal(24, result.PageSize);
    }

    [Fact]
    public async Task SearchAsync_MapsEachProductAndTheTotalCount()
    {
        var product = MakeProduct();
        _provider.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<Product> { product }, 42));

        var result = await CreateService().SearchAsync("jamdani", 1, 12, Ct);

        Assert.Equal(42, result.TotalCount);
        var dto = Assert.Single(result.Items);
        Assert.Equal(product.Id, dto.Id);
        Assert.Equal("Jamdani Saree", dto.Name);
        Assert.Equal("Weaving", dto.CategoryName);
        Assert.Equal("Dhaka", dto.DistrictName);
        Assert.Equal("Rahima Begum", dto.ProducerName);
    }

    [Fact]
    public async Task SearchAsync_NoImages_PrimaryImageUrlIsNull()
    {
        var product = MakeProduct();
        _provider.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<Product> { product }, 1));

        var dto = Assert.Single((await CreateService().SearchAsync("saree", 1, 12, Ct)).Items);

        Assert.Null(dto.PrimaryImageUrl);
    }

    [Fact]
    public async Task SearchAsync_SeveralImages_PicksTheLowestDisplayOrder()
    {
        var product = MakeProduct();
        product.Images.Add(new ProductImage { ImageUrl = "second.jpg", DisplayOrder = 2 });
        product.Images.Add(new ProductImage { ImageUrl = "first.jpg", DisplayOrder = 1 });
        _provider.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((new List<Product> { product }, 1));

        var dto = Assert.Single((await CreateService().SearchAsync("saree", 1, 12, Ct)).Items);

        Assert.Equal("first.jpg", dto.PrimaryImageUrl);
    }

    [Fact]
    public async Task SearchAsync_NoResults_ReturnsAnEmptyItemsListWithTheTotalCount()
    {
        _provider.SearchAsync(Arg.Any<string>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns((new List<Product>(), 0));

        var result = await CreateService().SearchAsync("nonexistent craft", 1, 12, Ct);

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalCount);
    }
}
