using ShilpoHubBD.Application.DTOs.ProducerComparison;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ProducerComparisonService = ShilpoHubBD.Application.Services.ProducerComparison.ProducerComparisonService;

namespace ShilpoHubBD.UnitTests.Features.ProducerComparison.Services;

[Trait("Feature", "ProducerComparison")]
[Trait("Layer", "Service")]
public class ProducerComparisonServiceTests
{
    private readonly IProducerComparisonRepository _repository = Substitute.For<IProducerComparisonRepository>();
    private readonly ProducerComparisonService _service;

    public ProducerComparisonServiceTests()
    {
        _service = new ProducerComparisonService(_repository);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task CompareAsync_AllProducersFound_ReturnsTheRows()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var rows = new List<ProducerComparisonRowDto> { new() { ProducerId = id1 }, new() { ProducerId = id2 } };
        _repository.CompareAsync(Arg.Is<List<Guid>>(ids => ids.Count == 2), Arg.Any<CancellationToken>()).Returns(rows);

        var result = await _service.CompareAsync(new ProducerComparisonRequest { ProducerIds = [id1, id2] }, Ct);

        Assert.Same(rows, result);
    }

    [Fact]
    public async Task CompareAsync_SomeProducersMissing_ThrowsNotFoundNamingTheMissingIds()
    {
        var found = Guid.NewGuid();
        var missing = Guid.NewGuid();
        _repository.CompareAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProducerComparisonRowDto> { new() { ProducerId = found } });

        var exception = await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CompareAsync(new ProducerComparisonRequest { ProducerIds = [found, missing] }, Ct));

        Assert.Contains(missing.ToString(), exception.Message);
    }

    [Fact]
    public async Task CompareAsync_DuplicateIdsInRequest_PassesOnlyDistinctIdsToTheRepository()
    {
        var id = Guid.NewGuid();
        _repository.CompareAsync(Arg.Any<List<Guid>>(), Arg.Any<CancellationToken>())
            .Returns(new List<ProducerComparisonRowDto> { new() { ProducerId = id } });

        await _service.CompareAsync(new ProducerComparisonRequest { ProducerIds = [id, id] }, Ct);

        await _repository.Received(1).CompareAsync(Arg.Is<List<Guid>>(ids => ids.Count == 1 && ids[0] == id), Ct);
    }
}
