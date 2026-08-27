using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Repositories;

public class InnovationExperimentRepository : IInnovationExperimentRepository
{
    private readonly ShilpoHubDbContext _context;

    public InnovationExperimentRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<InnovationExperiment?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.InnovationExperiments
            .Include(e => e.Owner)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<InnovationExperiment?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        => _context.InnovationExperiments
            .Include(e => e.Owner)
            .Include(e => e.Versions).ThenInclude(v => v.CreatedBy)
            .Include(e => e.Runs).ThenInclude(r => r.TriggeredBy)
            .Include(e => e.Runs).ThenInclude(r => r.ExperimentVersion)
            .AsSplitQuery()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<(List<InnovationExperiment> Items, int TotalCount)> GetPagedForOwnerAsync(
        Guid ownerUserId, InnovationExperimentQueryParameters query, CancellationToken cancellationToken)
    {
        var experiments = _context.InnovationExperiments
            .Include(e => e.Owner)
            .Where(e => e.OwnerUserId == ownerUserId);

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<InnovationExperimentStatus>(query.Status, true, out var status))
        {
            experiments = experiments.Where(e => e.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.ModelType)
            && Enum.TryParse<InnovationModelType>(query.ModelType, true, out var modelType))
        {
            experiments = experiments.Where(e => e.ModelType == modelType);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            experiments = experiments.Where(e => e.Name.ToLower().Contains(term) || e.Objective.ToLower().Contains(term));
        }

        experiments = experiments.OrderByDescending(e => e.UpdatedAt);

        var totalCount = await experiments.CountAsync(cancellationToken);
        var items = await experiments
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(InnovationExperiment experiment, CancellationToken cancellationToken)
        => await _context.InnovationExperiments.AddAsync(experiment, cancellationToken);

    public void Remove(InnovationExperiment experiment)
        => _context.InnovationExperiments.Remove(experiment);

    public Task<InnovationExperimentVersion?> GetVersionByIdAsync(Guid versionId, CancellationToken cancellationToken)
        => _context.InnovationExperimentVersions
            .Include(v => v.CreatedBy)
            .FirstOrDefaultAsync(v => v.Id == versionId, cancellationToken);

    public async Task<int> GetMaxVersionNumberAsync(Guid experimentId, CancellationToken cancellationToken)
        => (await _context.InnovationExperimentVersions
            .Where(v => v.InnovationExperimentId == experimentId)
            .Select(v => (int?)v.VersionNumber)
            .MaxAsync(cancellationToken)) ?? 0;

    public Task<List<InnovationExperimentVersion>> GetVersionsAsync(Guid experimentId, CancellationToken cancellationToken)
        => _context.InnovationExperimentVersions
            .Include(v => v.CreatedBy)
            .Where(v => v.InnovationExperimentId == experimentId)
            .OrderByDescending(v => v.VersionNumber)
            .ToListAsync(cancellationToken);

    public async Task AddVersionAsync(InnovationExperimentVersion version, CancellationToken cancellationToken)
        => await _context.InnovationExperimentVersions.AddAsync(version, cancellationToken);

    public Task<TrainingRun?> GetRunByIdAsync(Guid runId, CancellationToken cancellationToken)
        => _context.TrainingRuns
            .Include(r => r.TriggeredBy)
            .Include(r => r.ExperimentVersion)
            .FirstOrDefaultAsync(r => r.Id == runId, cancellationToken);

    public async Task<int> GetMaxRunNumberAsync(Guid experimentId, CancellationToken cancellationToken)
        => (await _context.TrainingRuns
            .Where(r => r.InnovationExperimentId == experimentId)
            .Select(r => (int?)r.RunNumber)
            .MaxAsync(cancellationToken)) ?? 0;

    public Task<List<TrainingRun>> GetRunsAsync(Guid experimentId, CancellationToken cancellationToken)
        => _context.TrainingRuns
            .Include(r => r.TriggeredBy)
            .Include(r => r.ExperimentVersion)
            .Where(r => r.InnovationExperimentId == experimentId)
            .OrderByDescending(r => r.RunNumber)
            .ToListAsync(cancellationToken);

    public async Task AddRunAsync(TrainingRun run, CancellationToken cancellationToken)
        => await _context.TrainingRuns.AddAsync(run, cancellationToken);

    public void RemoveRun(TrainingRun run)
        => _context.TrainingRuns.Remove(run);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
