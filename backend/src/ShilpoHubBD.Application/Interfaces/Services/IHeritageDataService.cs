using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.HeritageDatabase;

namespace ShilpoHubBD.Application.Interfaces.Services;

/// <summary>Live, read-only heritage data projections for researchers.</summary>
public interface IHeritageDataService
{
    Task<PagedResult<HeritageLocationRecordDto>> GetLocationsAsync(LiveHeritageQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<HeritageVillageRecordDto>> GetVillagesAsync(LiveHeritageQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<HeritageProducerRecordDto>> GetProducersAsync(LiveHeritageQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<HeritageProductRecordDto>> GetProductsAsync(LiveHeritageQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<HeritageTourismRecordDto>> GetTourismAsync(LiveHeritageQueryParameters query, CancellationToken cancellationToken);
    Task<ProducerDemographicsDto> GetProducerDemographicsAsync(CancellationToken cancellationToken);
    Task<HeritageDatabaseSummaryDto> GetSummaryAsync(CancellationToken cancellationToken);
}
