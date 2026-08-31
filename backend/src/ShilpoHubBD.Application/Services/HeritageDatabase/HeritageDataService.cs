using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;

namespace ShilpoHubBD.Application.Services.HeritageDatabase;

public class HeritageDataService : IHeritageDataService
{
    private readonly IHeritageDataRepository _repository;

    public HeritageDataService(IHeritageDataRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<HeritageLocationRecordDto>> GetLocationsAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        Normalize(query);
        var (items, total) = await _repository.GetLocationsAsync(query, cancellationToken);
        return Page(items, total, query);
    }

    public async Task<PagedResult<HeritageVillageRecordDto>> GetVillagesAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        Normalize(query);
        var (items, total) = await _repository.GetVillagesAsync(query, cancellationToken);
        return Page(items, total, query);
    }

    public async Task<PagedResult<HeritageProducerRecordDto>> GetProducersAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        Normalize(query);
        var (items, total) = await _repository.GetProducersAsync(query, cancellationToken);
        return Page(items, total, query);
    }

    public async Task<PagedResult<HeritageProductRecordDto>> GetProductsAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        Normalize(query);
        var (items, total) = await _repository.GetProductsAsync(query, cancellationToken);
        return Page(items, total, query);
    }

    public async Task<PagedResult<HeritageTourismRecordDto>> GetTourismAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken)
    {
        Normalize(query);
        var (items, total) = await _repository.GetTourismAsync(query, cancellationToken);
        return Page(items, total, query);
    }

    public Task<ProducerDemographicsDto> GetProducerDemographicsAsync(CancellationToken cancellationToken)
        => _repository.GetProducerDemographicsAsync(cancellationToken);

    public Task<HeritageDatabaseSummaryDto> GetSummaryAsync(CancellationToken cancellationToken)
        => _repository.GetSummaryAsync(cancellationToken);

    private static void Normalize(LiveHeritageQueryParameters query)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 200 ? 25 : query.PageSize;
    }

    private static PagedResult<T> Page<T>(List<T> items, int total, LiveHeritageQueryParameters query) => new()
    {
        Items = items,
        TotalCount = total,
        Page = query.Page,
        PageSize = query.PageSize,
    };
}
