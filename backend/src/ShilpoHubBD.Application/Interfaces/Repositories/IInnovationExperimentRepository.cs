using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IInnovationExperimentRepository
{
    Task<InnovationExperiment?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<InnovationExperiment?> GetDetailAsync(Guid id, CancellationToken cancellationToken);
    Task<(List<InnovationExperiment> Items, int TotalCount)> GetPagedForOwnerAsync(
        Guid ownerUserId, InnovationExperimentQueryParameters query, CancellationToken cancellationToken);
    Task AddAsync(InnovationExperiment experiment, CancellationToken cancellationToken);
    void Remove(InnovationExperiment experiment);

    Task<InnovationExperimentVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken);
    Task<int> GetMaxVersionNumberAsync(Guid experimentId, CancellationToken cancellationToken);
    Task<List<InnovationExperimentVersion>> GetVersionsAsync(Guid experimentId, CancellationToken cancellationToken);
    Task AddVersionAsync(InnovationExperimentVersion version, CancellationToken cancellationToken);

    Task<TrainingRun?> GetRunByIdAsync(Guid runId, CancellationToken cancellationToken);
    Task<int> GetMaxRunNumberAsync(Guid experimentId, CancellationToken cancellationToken);
    Task<List<TrainingRun>> GetRunsAsync(Guid experimentId, CancellationToken cancellationToken);
    Task AddRunAsync(TrainingRun run, CancellationToken cancellationToken);
    void RemoveRun(TrainingRun run);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
