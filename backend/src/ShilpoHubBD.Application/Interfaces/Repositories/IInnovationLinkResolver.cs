namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>Cross-module existence / access checks used when Innovation Lab records link to
/// Research Projects, Heritage Datasets, and to each other.</summary>
public interface IInnovationLinkResolver
{
    Task<bool> IsResearchProjectMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);

    Task<bool> IsDatasetAccessibleAsync(
        Guid datasetId, Guid userId, bool isResearcher, CancellationToken cancellationToken);

    Task<bool> PreservationStrategyOwnedByAsync(Guid strategyId, Guid userId, CancellationToken cancellationToken);

    Task<bool> InnovationExperimentOwnedByAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken);

    Task<bool> InnovationPrototypeOwnedByAsync(Guid prototypeId, Guid userId, CancellationToken cancellationToken);

    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken);
}
