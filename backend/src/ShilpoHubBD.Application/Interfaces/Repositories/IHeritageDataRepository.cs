using ShilpoHubBD.Application.DTOs.HeritageDatabase;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>Read-only projections of the live platform data for the National Heritage Database.</summary>
public interface IHeritageDataRepository
{
    Task<(List<HeritageLocationRecordDto> Items, int TotalCount)> GetLocationsAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken);

    Task<(List<HeritageVillageRecordDto> Items, int TotalCount)> GetVillagesAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken);

    Task<(List<HeritageProducerRecordDto> Items, int TotalCount)> GetProducersAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken);

    Task<(List<HeritageProductRecordDto> Items, int TotalCount)> GetProductsAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken);

    Task<(List<HeritageTourismRecordDto> Items, int TotalCount)> GetTourismAsync(
        LiveHeritageQueryParameters query, CancellationToken cancellationToken);

    Task<ProducerDemographicsDto> GetProducerDemographicsAsync(CancellationToken cancellationToken);

    Task<HeritageDatabaseSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);

    Task<int> CountLiveRecordsAsync(
        Domain.Entities.HeritageDatabase.HeritageDatasetCategory category, CancellationToken cancellationToken);
}
