using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Data.Interceptors;

/// <summary>
/// Marks a product "dirty" for the vector index whenever something the AI search depends on changes, in the same
/// transaction as the change itself. No service has to remember to do this, and no existing module is modified.
/// Only bookkeeping rows are written here; the embedding itself is done later by the sync worker.
/// </summary>
public sealed class ProductIndexDirtyInterceptor : SaveChangesInterceptor
{
    // Counters that change on every view / sale must not trigger re-indexing.
    private static readonly HashSet<string> IgnoredProductProperties = new()
    {
        nameof(Product.ViewCount), nameof(Product.SalesCount), nameof(Product.UpdatedAt), nameof(Product.LowStockThreshold),
    };

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context)
        {
            var (changed, deleted) = Collect(context);
            if (changed.Count > 0)
            {
                var existing = await context.Set<ProductIndexState>()
                    .Where(s => changed.Contains(s.ProductId))
                    .ToDictionaryAsync(s => s.ProductId, cancellationToken);
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
                var existing = context.Set<ProductIndexState>()
                    .Where(s => changed.Contains(s.ProductId))
                    .ToDictionary(s => s.ProductId);
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
                case Product product:
                    if (entry.State == EntityState.Deleted)
                    {
                        deleted.Add(product.Id);
                        changed.Add(product.Id);
                    }
                    else if (entry.State == EntityState.Added || HasIndexRelevantChange(entry))
                    {
                        changed.Add(product.Id);
                    }

                    break;
                case ProductAttributes attributes:
                    changed.Add(attributes.ProductId);
                    break;
                case ProductMaterial material:
                    changed.Add(material.ProductId);
                    break;
                case ProductImage image:
                    changed.Add(image.ProductId);
                    break;
                case ProductVariant variant:
                    changed.Add(variant.ProductId);
                    break;
            }
        }

        changed.Remove(Guid.Empty);
        return (changed, deleted);
    }

    private static bool HasIndexRelevantChange(EntityEntry entry)
        => entry.Properties.Any(p => p.IsModified && !p.Metadata.ValueGenerated.HasFlag(Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate)
            && !IgnoredProductProperties.Contains(p.Metadata.Name));

    private static void Apply(DbContext context, HashSet<Guid> changed, HashSet<Guid> deleted, Dictionary<Guid, ProductIndexState> existing)
    {
        var now = DateTime.UtcNow;
        var states = context.Set<ProductIndexState>();

        foreach (var id in changed)
        {
            if (!existing.TryGetValue(id, out var state))
            {
                state = new ProductIndexState { ProductId = id };
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
