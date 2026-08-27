using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IInnovationExperimentService
{
    Task<PagedResult<InnovationExperimentListItemDto>> GetMineAsync(
        Guid userId, InnovationExperimentQueryParameters query, CancellationToken cancellationToken);

    Task<InnovationExperimentDetailDto> GetByIdAsync(Guid userId, Guid experimentId, CancellationToken cancellationToken);

    Task<InnovationExperimentDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreateInnovationExperimentRequest request, CancellationToken cancellationToken);

    Task<InnovationExperimentDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid experimentId, UpdateInnovationExperimentRequest request, CancellationToken cancellationToken);

    Task DeleteAsync(Guid userId, Guid experimentId, CancellationToken cancellationToken);

    Task<ExperimentVersionDto> AddVersionAsync(
        Guid userId, Guid experimentId, CreateExperimentVersionRequest request, CancellationToken cancellationToken);

    Task<TrainingRunDto> CreateRunAsync(
        Guid userId, Guid experimentId, CreateTrainingRunRequest request, CancellationToken cancellationToken);

    Task<TrainingRunDto> UpdateRunAsync(
        Guid userId, Guid experimentId, Guid runId, UpdateTrainingRunRequest request, CancellationToken cancellationToken);

    Task DeleteRunAsync(Guid userId, Guid experimentId, Guid runId, CancellationToken cancellationToken);
}
