using ShilpoHubBD.Application.DTOs.Traceability;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Traceability;
using TraceabilityService = ShilpoHubBD.Application.Services.Traceability.TraceabilityService;

namespace ShilpoHubBD.UnitTests.Features.Traceability.Services;

[Trait("Feature", "Traceability")]
[Trait("Layer", "Service")]
public class TraceabilityServiceTests
{
    private readonly ITraceabilityRepository _repository = Substitute.For<ITraceabilityRepository>();
    private readonly IProductRepository _productRepository = Substitute.For<IProductRepository>();
    private readonly TraceabilityService _service;

    public TraceabilityServiceTests()
    {
        _service = new TraceabilityService(_repository, _productRepository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Product MakeProduct(Guid producerId) => new() { Id = Guid.NewGuid(), Name = "Nakshi Kantha", ProducerId = producerId };

    private static ProductTraceability MakeTraceability(Product product) => new()
    {
        Id = Guid.NewGuid(),
        ProductId = product.Id,
        Product = product,
        Summary = "Handwoven from local cotton.",
    };

    [Fact]
    public async Task GetByProductIdAsync_UnknownProduct_ThrowsNotFound()
    {
        _repository.GetByProductIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProductTraceability?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetByProductIdAsync(Guid.NewGuid(), Ct));
    }

    [Fact]
    public async Task GetByProductIdAsync_Known_ReturnsMappedDto()
    {
        var product = MakeProduct(Guid.NewGuid());
        var traceability = MakeTraceability(product);
        traceability.MaterialSources.Add(new MaterialSource { Id = Guid.NewGuid(), MaterialName = "Cotton", SourceLocation = "Rangpur", Description = "Local cotton", DisplayOrder = 0 });
        traceability.TimelineEvents.Add(new TimelineEvent { Id = Guid.NewGuid(), Title = "Woven", Description = "Woven by hand", EventDate = DateTime.UtcNow, DisplayOrder = 0 });
        _repository.GetByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(traceability);

        var result = await _service.GetByProductIdAsync(product.Id, Ct);

        Assert.Equal(traceability.Id, result.Id);
        Assert.Equal(product.Name, result.ProductName);
        Assert.Single(result.MaterialSources);
        Assert.Single(result.TimelineEvents);
    }

    [Fact]
    public async Task CreateAsync_UnknownProduct_ThrowsNotFound()
    {
        _productRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Product?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateAsync(Guid.NewGuid(), new CreateProductTraceabilityRequest { ProductId = Guid.NewGuid() }, Ct));
    }

    [Fact]
    public async Task CreateAsync_NotYourProduct_ThrowsUnauthorized()
    {
        var product = MakeProduct(Guid.NewGuid());
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.CreateAsync(Guid.NewGuid(), new CreateProductTraceabilityRequest { ProductId = product.Id }, Ct));
    }

    [Fact]
    public async Task CreateAsync_AlreadyHasARecord_ThrowsConflict()
    {
        var producerId = Guid.NewGuid();
        var product = MakeProduct(producerId);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.ExistsByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(true);

        await Assert.ThrowsAsync<ConflictException>(() =>
            _service.CreateAsync(producerId, new CreateProductTraceabilityRequest { ProductId = product.Id }, Ct));
    }

    [Fact]
    public async Task CreateAsync_Valid_SavesWithTrimmedSummaryAndOrderedChildRows()
    {
        var producerId = Guid.NewGuid();
        var product = MakeProduct(producerId);
        _productRepository.GetByIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(product);
        _repository.ExistsByProductIdAsync(product.Id, Arg.Any<CancellationToken>()).Returns(false);

        var request = new CreateProductTraceabilityRequest
        {
            ProductId = product.Id,
            Summary = "  Handwoven from local cotton.  ",
            MaterialSources =
            [
                new MaterialSourceInput { MaterialName = " Cotton ", SourceLocation = " Rangpur ", Description = " Local cotton " },
                new MaterialSourceInput { MaterialName = "Dye", SourceLocation = "Dhaka", Description = "Natural dye" },
            ],
            TimelineEvents =
            [
                new TimelineEventInput { Title = " Woven ", Description = " Woven by hand ", Location = "   ", EventDate = DateTime.UtcNow },
            ],
        };

        var result = await _service.CreateAsync(producerId, request, Ct);

        await _repository.Received(1).AddAsync(Arg.Is<ProductTraceability>(t =>
            t.ProductId == product.Id && t.Summary == "Handwoven from local cotton."
            && t.MaterialSources.Count == 2
            && t.MaterialSources.ElementAt(0).MaterialName == "Cotton" && t.MaterialSources.ElementAt(0).DisplayOrder == 0
            && t.MaterialSources.ElementAt(1).DisplayOrder == 1
            && t.TimelineEvents.Single().Title == "Woven" && t.TimelineEvents.Single().Location == null), Ct);
        await _repository.Received(1).SaveChangesAsync(Ct);
        Assert.Equal(product.Name, result.ProductName);
    }

    [Fact]
    public async Task UpdateAsync_UnknownRecord_ThrowsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProductTraceability?)null);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.UpdateAsync(Guid.NewGuid(), Guid.NewGuid(), false, new UpdateProductTraceabilityRequest(), Ct));
    }

    [Fact]
    public async Task UpdateAsync_NotYourProductAndNotAdmin_ThrowsUnauthorized()
    {
        var traceability = MakeTraceability(MakeProduct(Guid.NewGuid()));
        _repository.GetByIdAsync(traceability.Id, Arg.Any<CancellationToken>()).Returns(traceability);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdateAsync(traceability.Id, Guid.NewGuid(), false, new UpdateProductTraceabilityRequest(), Ct));
    }

    [Fact]
    public async Task UpdateAsync_Admin_CanUpdateSomeoneElsesRecord()
    {
        var traceability = MakeTraceability(MakeProduct(Guid.NewGuid()));
        _repository.GetByIdAsync(traceability.Id, Arg.Any<CancellationToken>()).Returns(traceability);

        var result = await _service.UpdateAsync(traceability.Id, Guid.NewGuid(), true, new UpdateProductTraceabilityRequest { Summary = "New summary" }, Ct);

        Assert.Equal("New summary", result.Summary);
    }

    [Fact]
    public async Task UpdateAsync_Valid_ReplacesMaterialSourcesAndTimelineEvents()
    {
        var producerId = Guid.NewGuid();
        var traceability = MakeTraceability(MakeProduct(producerId));
        traceability.MaterialSources.Add(new MaterialSource { Id = Guid.NewGuid(), MaterialName = "Old", SourceLocation = "x", Description = "x" });
        traceability.TimelineEvents.Add(new TimelineEvent { Id = Guid.NewGuid(), Title = "Old", Description = "x", EventDate = DateTime.UtcNow });
        _repository.GetByIdAsync(traceability.Id, Arg.Any<CancellationToken>()).Returns(traceability);

        var request = new UpdateProductTraceabilityRequest
        {
            Summary = "  Updated summary  ",
            MaterialSources = [new MaterialSourceInput { MaterialName = "New", SourceLocation = "Dhaka", Description = "x" }],
            TimelineEvents = [],
        };

        var result = await _service.UpdateAsync(traceability.Id, producerId, false, request, Ct);

        Assert.Equal("Updated summary", result.Summary);
        Assert.Equal("New", Assert.Single(result.MaterialSources).MaterialName);
        Assert.Empty(result.TimelineEvents);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }

    [Fact]
    public async Task DeleteAsync_UnknownRecord_ThrowsNotFound()
    {
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((ProductTraceability?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync(Guid.NewGuid(), Guid.NewGuid(), false, Ct));
    }

    [Fact]
    public async Task DeleteAsync_NotYourProductAndNotAdmin_ThrowsUnauthorized()
    {
        var traceability = MakeTraceability(MakeProduct(Guid.NewGuid()));
        _repository.GetByIdAsync(traceability.Id, Arg.Any<CancellationToken>()).Returns(traceability);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.DeleteAsync(traceability.Id, Guid.NewGuid(), false, Ct));
    }

    [Fact]
    public async Task DeleteAsync_Valid_RemovesAndSaves()
    {
        var producerId = Guid.NewGuid();
        var traceability = MakeTraceability(MakeProduct(producerId));
        _repository.GetByIdAsync(traceability.Id, Arg.Any<CancellationToken>()).Returns(traceability);

        await _service.DeleteAsync(traceability.Id, producerId, false, Ct);

        _repository.Received(1).Remove(traceability);
        await _repository.Received(1).SaveChangesAsync(Ct);
    }
}
