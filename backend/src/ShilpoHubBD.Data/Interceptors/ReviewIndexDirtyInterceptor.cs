using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShilpoHubBD.Domain.Entities.ProductSearch;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Data.Interceptors;

/// <summary>
/// Marks a product review "dirty" for the review vector index whenever the review itself is created/edited or
/// its AI analysis is written, in the same transaction as the change — mirrors <see cref="ProductIndexDirtyInterceptor"/>.
/// No service has to remember to do this, and neither <c>ReviewService</c> nor <c>ReviewAiAnalysisService</c>
/// needs to know the index exists. Only bookkeeping rows are written here; embedding happens later, out of process.
/// </summary>
public sealed class ReviewIndexDirtyInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            var (changed, deleted) = Collect(context);
            if (changed.Count > 0)
            {
                var existing = await context.Set<ReviewIndexState>()
                    .Where(s => changed.Contains(s.ReviewId))
                    .ToDictionaryAsync(s => s.ReviewId, cancellationToken);
                Apply(context, changed, deleted, existing);
            }
        }

        return result;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (eventData.Context is { } context)
        {
            var (changed, deleted) = Collect(context);
            if (changed.Count > 0)
            {
                var existing = context.Set<ReviewIndexState>()
                    .Where(s => changed.Contains(s.ReviewId))
                    .ToDictionary(s => s.ReviewId);
                Apply(context, changed, deleted, existing);
            }
        }

        return result;
    }

    private static (HashSet<Guid> Changed, HashSet<Guid> Deleted) Collect(DbContext context)
    {
        var changed = new HashSet<Guid>();
        var deleted = new HashSet<Guid>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            switch (entry.Entity)
            {
                // Only product reviews are indexed; heritage-place and booking reviews are out of scope here.
                case Review review when review.ProductId.HasValue:
                    if (entry.State == EntityState.Deleted)
                    {
                        deleted.Add(review.Id);
                        changed.Add(review.Id);
                    }
                    else if (entry.State == EntityState.Added || HasIndexRelevantChange(entry))
                    {
                        changed.Add(review.Id);
                    }

                    break;
                case ReviewAiAnalysis analysis when entry.State != EntityState.Deleted:
                    changed.Add(analysis.ReviewId);
                    break;
            }
        }

        changed.Remove(Guid.Empty);
        return (changed, deleted);
    }

    private static bool HasIndexRelevantChange(EntityEntry entry)
        => entry.Properties.Any(p => p.IsModified && p.Metadata.Name == nameof(Review.Comment));

    private static void Apply(DbContext context, HashSet<Guid> changed, HashSet<Guid> deleted, Dictionary<Guid, ReviewIndexState> existing)
    {
        var now = DateTime.UtcNow;
        var states = context.Set<ReviewIndexState>();

        foreach (var id in changed)
        {
            if (!existing.TryGetValue(id, out var state))
            {
                state = new ReviewIndexState { ReviewId = id };
                states.Add(state);
            }

            state.Status = deleted.Contains(id) ? IndexStatuses.Deleted : IndexStatuses.Dirty;
            state.Version++;
            state.AttemptCount = 0;
            state.LastError = null;
            state.UpdatedAt = now;
        }

        // The interceptor runs after EF's own change detection; detect again so the edits above are saved.
        context.ChangeTracker.DetectChanges();
    }
}
