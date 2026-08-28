using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Application.Services.Innovation;

public class InnovationExperimentService : IInnovationExperimentService
{
    private readonly IInnovationExperimentRepository _repository;
    private readonly IInnovationLinkResolver _links;

    public InnovationExperimentService(IInnovationExperimentRepository repository, IInnovationLinkResolver links)
    {
        _repository = repository;
        _links = links;
    }

    public async Task<PagedResult<InnovationExperimentListItemDto>> GetMineAsync(
        Guid userId, InnovationExperimentQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedForOwnerAsync(userId, query, cancellationToken);
        return new PagedResult<InnovationExperimentListItemDto>
        {
            Items = items.Select(e => e.ToListItemDto()).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<InnovationExperimentDetailDto> GetByIdAsync(Guid userId, Guid experimentId, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetDetailAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);
        return experiment.ToDetailDto();
    }

    public async Task<InnovationExperimentDetailDto> CreateAsync(
        Guid userId, bool isResearcher, CreateInnovationExperimentRequest request, CancellationToken cancellationToken)
    {
        var modelType = ParseModelType(request.ModelType);
        await ValidateLinksAsync(userId, isResearcher, request.ResearchProjectId, request.HeritageDatasetId, cancellationToken);

        var now = DateTime.UtcNow;
        var experiment = new InnovationExperiment
        {
            Id = Guid.NewGuid(),
            OwnerUserId = userId,
            ResearchProjectId = request.ResearchProjectId,
            HeritageDatasetId = request.HeritageDatasetId,
            Name = request.Name.Trim(),
            Objective = request.Objective.Trim(),
            Description = request.Description?.Trim(),
            ModelType = modelType,
            Framework = request.Framework?.Trim(),
            ConfigJson = request.ConfigJson,
            Status = InnovationExperimentStatus.Draft,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(experiment, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetDetailAsync(experiment.Id, cancellationToken))!.ToDetailDto();
    }

    public async Task<InnovationExperimentDetailDto> UpdateAsync(
        Guid userId, bool isResearcher, Guid experimentId, UpdateInnovationExperimentRequest request, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetDetailAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);

        var modelType = ParseModelType(request.ModelType);
        if (!Enum.TryParse<InnovationExperimentStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Draft, Active, Archived.");
        }

        await ValidateLinksAsync(userId, isResearcher, request.ResearchProjectId, request.HeritageDatasetId, cancellationToken);

        experiment.Name = request.Name.Trim();
        experiment.Objective = request.Objective.Trim();
        experiment.Description = request.Description?.Trim();
        experiment.ModelType = modelType;
        experiment.Framework = request.Framework?.Trim();
        experiment.ConfigJson = request.ConfigJson;
        experiment.Status = status;
        experiment.ResearchProjectId = request.ResearchProjectId;
        experiment.HeritageDatasetId = request.HeritageDatasetId;
        experiment.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return experiment.ToDetailDto();
    }

    public async Task DeleteAsync(Guid userId, Guid experimentId, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetByIdAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);

        _repository.Remove(experiment);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    public async Task<ExperimentVersionDto> AddVersionAsync(
        Guid userId, Guid experimentId, CreateExperimentVersionRequest request, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetDetailAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);

        var nextNumber = await _repository.GetMaxVersionNumberAsync(experimentId, cancellationToken) + 1;
        var now = DateTime.UtcNow;
        var version = new InnovationExperimentVersion
        {
            Id = Guid.NewGuid(),
            InnovationExperimentId = experimentId,
            VersionNumber = nextNumber,
            Label = string.IsNullOrWhiteSpace(request.Label) ? $"v{nextNumber}" : request.Label.Trim(),
            Notes = request.Notes.Trim(),
            ConfigJson = request.ConfigJson,
            Framework = request.Framework?.Trim(),
            ArtifactUrl = request.ArtifactUrl?.Trim(),
            IsCurrent = request.SetAsCurrent,
            CreatedByUserId = userId,
            CreatedAt = now,
        };

        if (request.SetAsCurrent)
        {
            foreach (var existing in experiment.Versions.Where(v => v.IsCurrent))
            {
                existing.IsCurrent = false;
            }
        }

        await _repository.AddVersionAsync(version, cancellationToken);
        experiment.VersionCount += 1;
        experiment.UpdatedAt = now;
        if (request.SetAsCurrent)
        {
            experiment.CurrentVersionId = version.Id;
        }

        if (experiment.Status == InnovationExperimentStatus.Draft)
        {
            experiment.Status = InnovationExperimentStatus.Active;
        }

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetVersionByIdAsync(version.Id, cancellationToken))!.ToDto();
    }

    public async Task<TrainingRunDto> CreateRunAsync(
        Guid userId, Guid experimentId, CreateTrainingRunRequest request, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetDetailAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);

        if (request.ExperimentVersionId.HasValue
            && experiment.Versions.All(v => v.Id != request.ExperimentVersionId.Value))
        {
            throw new NotFoundException("Experiment version not found.");
        }

        var nextNumber = await _repository.GetMaxRunNumberAsync(experimentId, cancellationToken) + 1;
        var now = DateTime.UtcNow;
        var run = new TrainingRun
        {
            Id = Guid.NewGuid(),
            InnovationExperimentId = experimentId,
            ExperimentVersionId = request.ExperimentVersionId,
            RunNumber = nextNumber,
            Status = TrainingRunStatus.Pending,
            DatasetSnapshotName = request.DatasetSnapshotName?.Trim(),
            HyperparametersJson = request.HyperparametersJson,
            Notes = request.Notes?.Trim(),
            TriggeredByUserId = userId,
            CreatedAt = now,
        };

        await _repository.AddRunAsync(run, cancellationToken);
        experiment.RunCount += 1;
        experiment.UpdatedAt = now;
        await _repository.SaveChangesAsync(cancellationToken);

        return (await _repository.GetRunByIdAsync(run.Id, cancellationToken))!.ToDto();
    }

    public async Task<TrainingRunDto> UpdateRunAsync(
        Guid userId, Guid experimentId, Guid runId, UpdateTrainingRunRequest request, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetByIdAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);

        var run = await _repository.GetRunByIdAsync(runId, cancellationToken);
        if (run is null || run.InnovationExperimentId != experimentId)
        {
            throw new NotFoundException("Training run not found.");
        }

        if (!Enum.TryParse<TrainingRunStatus>(request.Status, true, out var status))
        {
            throw new ConflictException("Status must be one of: Pending, Running, Completed, Failed, Cancelled.");
        }

        run.Status = status;
        run.DatasetSnapshotName = request.DatasetSnapshotName?.Trim();
        run.HyperparametersJson = request.HyperparametersJson;
        run.MetricsJson = request.MetricsJson;
        run.PrimaryMetricName = request.PrimaryMetricName?.Trim();
        run.PrimaryMetricValue = request.PrimaryMetricValue;
        run.Notes = request.Notes?.Trim();
        run.StartedAt = request.StartedAt;
        run.CompletedAt = request.CompletedAt
            ?? (status is TrainingRunStatus.Completed or TrainingRunStatus.Failed or TrainingRunStatus.Cancelled
                ? DateTime.UtcNow
                : null);

        await _repository.SaveChangesAsync(cancellationToken);
        return (await _repository.GetRunByIdAsync(run.Id, cancellationToken))!.ToDto();
    }

    public async Task DeleteRunAsync(Guid userId, Guid experimentId, Guid runId, CancellationToken cancellationToken)
    {
        var experiment = await _repository.GetByIdAsync(experimentId, cancellationToken)
            ?? throw new NotFoundException("Experiment not found.");
        RequireOwner(experiment, userId);

        var run = await _repository.GetRunByIdAsync(runId, cancellationToken);
        if (run is null || run.InnovationExperimentId != experimentId)
        {
            throw new NotFoundException("Training run not found.");
        }

        _repository.RemoveRun(run);
        if (experiment.RunCount > 0)
        {
            experiment.RunCount -= 1;
        }

        experiment.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync(cancellationToken);
    }

    // ---- helpers ----

    private static void RequireOwner(InnovationExperiment experiment, Guid userId)
    {
        if (experiment.OwnerUserId != userId)
        {
            throw new NotFoundException("Experiment not found.");
        }
    }

    private async Task ValidateLinksAsync(
        Guid userId, bool isResearcher, Guid? projectId, Guid? datasetId, CancellationToken cancellationToken)
    {
        if (projectId.HasValue && !await _links.IsResearchProjectMemberAsync(projectId.Value, userId, cancellationToken))
        {
            throw new ConflictException("You can only link a research project you belong to.");
        }

        if (datasetId.HasValue && !await _links.IsDatasetAccessibleAsync(datasetId.Value, userId, isResearcher, cancellationToken))
        {
            throw new ConflictException("You do not have access to the linked dataset.");
        }
    }

    private static InnovationModelType ParseModelType(string value)
        => Enum.TryParse<InnovationModelType>(value, true, out var parsed)
            ? parsed
            : throw new ConflictException("ModelType is not a valid model type.");
}
