using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;

namespace ShilpoHubBD.Data.Repositories;

public class InnovationLinkResolver : IInnovationLinkResolver
{
    private readonly ShilpoHubDbContext _context;

    public InnovationLinkResolver(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<bool> IsResearchProjectMemberAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
        => _context.ResearchProjectMembers.AnyAsync(
            m => m.ResearchProjectId == projectId && m.UserId == userId, cancellationToken);

    public Task<bool> IsDatasetAccessibleAsync(
        Guid datasetId, Guid userId, bool isResearcher, CancellationToken cancellationToken)
        => _context.HeritageDatasets.AnyAsync(d => d.Id == datasetId && (
            d.AccessLevel == HeritageDatasetAccessLevel.Public
            || d.OwnerUserId == userId
            || d.AccessGrants.Any(g => g.UserId == userId && (g.ExpiresAt == null || g.ExpiresAt > DateTime.UtcNow))
            || (isResearcher && d.AccessLevel == HeritageDatasetAccessLevel.Researcher)),
            cancellationToken);

    public Task<bool> PreservationStrategyOwnedByAsync(Guid strategyId, Guid userId, CancellationToken cancellationToken)
        => _context.PreservationStrategies.AnyAsync(
            s => s.Id == strategyId && s.OwnerUserId == userId, cancellationToken);

    public Task<bool> InnovationExperimentOwnedByAsync(Guid experimentId, Guid userId, CancellationToken cancellationToken)
        => _context.InnovationExperiments.AnyAsync(
            e => e.Id == experimentId && e.OwnerUserId == userId, cancellationToken);

    public Task<bool> InnovationPrototypeOwnedByAsync(Guid prototypeId, Guid userId, CancellationToken cancellationToken)
        => _context.InnovationPrototypes.AnyAsync(
            p => p.Id == prototypeId && p.OwnerUserId == userId, cancellationToken);

    public Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken)
        => _context.Users.AnyAsync(u => u.Id == userId, cancellationToken);
}
