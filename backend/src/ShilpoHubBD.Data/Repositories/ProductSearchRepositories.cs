using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Data.Repositories;

public class ProductLookupRepository : IProductLookupRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProductLookupRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<List<ProductType>> GetTypesAsync(bool includeInactive, CancellationToken cancellationToken)
        => _context.ProductTypes.Where(t => includeInactive || t.IsActive).OrderBy(t => t.DisplayOrder).ThenBy(t => t.Name).ToListAsync(cancellationToken);

    public Task<ProductType?> GetTypeAsync(Guid id, CancellationToken cancellationToken)
        => _context.ProductTypes.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public Task<bool> TypeSlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken)
        => _context.ProductTypes.AnyAsync(t => t.Slug == slug && (exceptId == null || t.Id != exceptId), cancellationToken);

    public async Task AddTypeAsync(ProductType type, CancellationToken cancellationToken) => await _context.ProductTypes.AddAsync(type, cancellationToken);

    public void RemoveType(ProductType type) => _context.ProductTypes.Remove(type);

    public Task<List<Material>> GetMaterialsAsync(bool includeInactive, CancellationToken cancellationToken)
        => _context.Materials.Where(m => includeInactive || m.IsActive).OrderBy(m => m.DisplayOrder).ThenBy(m => m.Name).ToListAsync(cancellationToken);

    public Task<Material?> GetMaterialAsync(Guid id, CancellationToken cancellationToken)
        => _context.Materials.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

    public Task<bool> MaterialSlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken)
        => _context.Materials.AnyAsync(m => m.Slug == slug && (exceptId == null || m.Id != exceptId), cancellationToken);

    public Task<bool> MaterialInUseAsync(Guid id, CancellationToken cancellationToken)
        => _context.ProductMaterials.AnyAsync(m => m.MaterialId == id, cancellationToken);

    public async Task AddMaterialAsync(Material material, CancellationToken cancellationToken) => await _context.Materials.AddAsync(material, cancellationToken);

    public void RemoveMaterial(Material material) => _context.Materials.Remove(material);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

public class ProductAttributesRepository : IProductAttributesRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProductAttributesRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<Guid?> GetProducerIdAsync(Guid productId, CancellationToken cancellationToken)
        => await _context.Products.Where(p => p.Id == productId).Select(p => (Guid?)p.ProducerId).FirstOrDefaultAsync(cancellationToken);

    public Task<Product?> GetProductWithAttributesAsync(Guid productId, CancellationToken cancellationToken)
        => _context.Products
            .Include(p => p.Category)
            .Include(p => p.District)
            .Include(p => p.ProductType)
            .Include(p => p.Attributes)
            .Include(p => p.Materials).ThenInclude(m => m.Material)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

    public Task<ProductType?> GetActiveTypeAsync(Guid id, CancellationToken cancellationToken)
        => _context.ProductTypes.FirstOrDefaultAsync(t => t.Id == id && t.IsActive, cancellationToken);

    public Task<List<Material>> GetActiveMaterialsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken)
        => _context.Materials.Where(m => ids.Contains(m.Id) && m.IsActive).ToListAsync(cancellationToken);

    public async Task AddAttributesAsync(ProductAttributes attributes, CancellationToken cancellationToken)
        => await _context.ProductAttributeRecords.AddAsync(attributes, cancellationToken);

    public Task<ProductAttributeSuggestion?> GetPendingSuggestionAsync(Guid productId, CancellationToken cancellationToken)
        => _context.ProductAttributeSuggestions.Where(s => s.ProductId == productId && s.Status == SuggestionStatus.Pending)
            .OrderByDescending(s => s.CreatedAt).FirstOrDefaultAsync(cancellationToken);

    public Task<ProductAttributeSuggestion?> GetSuggestionAsync(Guid suggestionId, CancellationToken cancellationToken)
        => _context.ProductAttributeSuggestions.FirstOrDefaultAsync(s => s.Id == suggestionId, cancellationToken);

    public Task<List<ProductAttributeSuggestion>> GetPendingSuggestionsForProductAsync(Guid productId, CancellationToken cancellationToken)
        => _context.ProductAttributeSuggestions.Where(s => s.ProductId == productId && s.Status == SuggestionStatus.Pending).ToListAsync(cancellationToken);

    public async Task AddSuggestionAsync(ProductAttributeSuggestion suggestion, CancellationToken cancellationToken)
        => await _context.ProductAttributeSuggestions.AddAsync(suggestion, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}

public class ProductIndexRepository : IProductIndexRepository
{
    private readonly ShilpoHubDbContext _context;

    public ProductIndexRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // Dirty rows first, then failed ones that still have attempts left.
    private IQueryable<ProductIndexState> Pending(int maxAttempts)
        => _context.ProductIndexStates.Where(s => s.Status == IndexStatuses.Dirty || s.Status == IndexStatuses.Deleted
            || (s.Status == IndexStatuses.Failed && s.AttemptCount < maxAttempts));

    public Task<List<ProductIndexState>> GetPendingAsync(int limit, int maxAttempts, CancellationToken cancellationToken)
        => Pending(maxAttempts).OrderBy(s => s.UpdatedAt).Take(limit).ToListAsync(cancellationToken);

    public Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken) => Pending(maxAttempts).CountAsync(cancellationToken);

    public Task<Dictionary<Guid, ProductIndexState>> GetStatesAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
        => _context.ProductIndexStates.Where(s => productIds.Contains(s.ProductId)).ToDictionaryAsync(s => s.ProductId, cancellationToken);

    public Task<Dictionary<string, int>> CountByStatusAsync(CancellationToken cancellationToken)
        => _context.ProductIndexStates.GroupBy(s => s.Status).Select(g => new { g.Key, Count = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

    public async Task<int> RequeueAllAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        // Every existing product gets a state, then all non-deleted states go back to Dirty.
        await _context.Database.ExecuteSqlInterpolatedAsync($@"
            INSERT INTO ""ProductIndexStates"" (""ProductId"", ""Status"", ""Version"", ""AttemptCount"", ""UpdatedAt"")
            SELECT p.""Id"", 'Dirty', 1, 0, {now} FROM ""Products"" p
            WHERE NOT EXISTS (SELECT 1 FROM ""ProductIndexStates"" s WHERE s.""ProductId"" = p.""Id"")", cancellationToken);
        return await _context.Database.ExecuteSqlInterpolatedAsync($@"
            UPDATE ""ProductIndexStates"" SET ""Status"" = 'Dirty', ""Version"" = ""Version"" + 1, ""AttemptCount"" = 0, ""LastError"" = NULL, ""UpdatedAt"" = {now}
            WHERE ""Status"" <> 'Deleted'", cancellationToken);
    }

    public async Task<Dictionary<Guid, Product>> GetProductsForIndexAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
        => (await _context.Products.AsNoTracking().AsSplitQuery()
            .Include(p => p.Category)
            .Include(p => p.District)
            .Include(p => p.Producer)
            .Include(p => p.ProductType)
            .Include(p => p.Attributes)
            .Include(p => p.Materials).ThenInclude(m => m.Material)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .Where(p => productIds.Contains(p.Id))
            .ToListAsync(cancellationToken)).ToDictionary(p => p.Id);

    public async Task<Dictionary<Guid, List<string>>> GetMaterialSourcesAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken)
    {
        var rows = await _context.ProductTraceabilities.AsNoTracking()
            .Where(t => productIds.Contains(t.ProductId))
            .SelectMany(t => t.MaterialSources.Select(m => new { t.ProductId, m.MaterialName, m.SourceLocation, m.DisplayOrder }))
            .ToListAsync(cancellationToken);
        return rows.GroupBy(r => r.ProductId).ToDictionary(
            g => g.Key,
            g => g.OrderBy(r => r.DisplayOrder).Select(r => string.IsNullOrWhiteSpace(r.SourceLocation) ? r.MaterialName : $"{r.MaterialName} from {r.SourceLocation}").ToList());
    }

    public async Task<Dictionary<Guid, string>> GetWorkshopNamesAsync(IReadOnlyCollection<Guid> producerIds, CancellationToken cancellationToken)
        => (await _context.Set<ShilpoHubBD.Domain.Entities.HeritageIdentity.ProducerHeritageIdentity>().AsNoTracking()
            .Where(i => producerIds.Contains(i.ProducerId) && i.WorkshopName != "")
            .Select(i => new { i.ProducerId, i.WorkshopName })
            .ToListAsync(cancellationToken)).GroupBy(i => i.ProducerId).ToDictionary(g => g.Key, g => g.First().WorkshopName);

    public async Task<Dictionary<string, CraftHeritageEntry>> GetCraftHeritageBySlugAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken)
        => (await _context.CraftHeritageEntries.AsNoTracking().Where(e => e.IsActive && slugs.Contains(e.Slug)).ToListAsync(cancellationToken))
            .ToDictionary(e => e.Slug, StringComparer.OrdinalIgnoreCase);

    public void RemoveState(ProductIndexState state) => _context.ProductIndexStates.Remove(state);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => _context.SaveChangesAsync(cancellationToken);
}
