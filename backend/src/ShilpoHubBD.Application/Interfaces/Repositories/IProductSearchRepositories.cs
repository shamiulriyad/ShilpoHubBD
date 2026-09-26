using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.ProductSearch;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IProductLookupRepository
{
    Task<List<ProductType>> GetTypesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<ProductType?> GetTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> TypeSlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken);
    Task AddTypeAsync(ProductType type, CancellationToken cancellationToken);
    void RemoveType(ProductType type);

    Task<List<Material>> GetMaterialsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<Material?> GetMaterialAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> MaterialSlugExistsAsync(string slug, Guid? exceptId, CancellationToken cancellationToken);
    Task<bool> MaterialInUseAsync(Guid id, CancellationToken cancellationToken);
    Task AddMaterialAsync(Material material, CancellationToken cancellationToken);
    void RemoveMaterial(Material material);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IProductAttributesRepository
{
    /// <summary>The producer id of a product, or null when the product does not exist.</summary>
    Task<Guid?> GetProducerIdAsync(Guid productId, CancellationToken cancellationToken);

    Task<Product?> GetProductWithAttributesAsync(Guid productId, CancellationToken cancellationToken);
    Task<ProductType?> GetActiveTypeAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Material>> GetActiveMaterialsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken);
    Task AddAttributesAsync(ProductAttributes attributes, CancellationToken cancellationToken);

    Task<ProductAttributeSuggestion?> GetPendingSuggestionAsync(Guid productId, CancellationToken cancellationToken);
    Task<ProductAttributeSuggestion?> GetSuggestionAsync(Guid suggestionId, CancellationToken cancellationToken);
    Task<List<ProductAttributeSuggestion>> GetPendingSuggestionsForProductAsync(Guid productId, CancellationToken cancellationToken);
    Task AddSuggestionAsync(ProductAttributeSuggestion suggestion, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}

public interface IProductSearchQueryRepository
{
    /// <summary>
    /// Public (active + approved) products matching the authoritative filters. With <paramref name="take"/> the result is
    /// sorted in the database by <see cref="ShilpoHubBD.Application.DTOs.ProductSearch.ProductSearchCriteria.Sort"/> and paged;
    /// without it (candidate mode) up to 300 rows come back for in-memory ranking.
    /// </summary>
    Task<(List<Product> Items, int Total)> SearchAsync(
        ShilpoHubBD.Application.DTOs.ProductSearch.ProductSearchCriteria criteria, int? skip, int? take, CancellationToken cancellationToken);
}

public interface IProductIndexRepository
{
    Task<List<ProductIndexState>> GetPendingAsync(int limit, int maxAttempts, CancellationToken cancellationToken);
    Task<int> CountPendingAsync(int maxAttempts, CancellationToken cancellationToken);
    Task<Dictionary<Guid, ProductIndexState>> GetStatesAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);
    Task<Dictionary<string, int>> CountByStatusAsync(CancellationToken cancellationToken);
    Task<int> RequeueAllAsync(CancellationToken cancellationToken);

    /// <summary>Products with everything the search document needs, keyed by id.</summary>
    Task<Dictionary<Guid, Product>> GetProductsForIndexAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    /// <summary>Traceability material sources ("Cotton yarn — Rajshahi") per product.</summary>
    Task<Dictionary<Guid, List<string>>> GetMaterialSourcesAsync(IReadOnlyCollection<Guid> productIds, CancellationToken cancellationToken);

    /// <summary>Workshop name per producer, from the heritage identity when one exists.</summary>
    Task<Dictionary<Guid, string>> GetWorkshopNamesAsync(IReadOnlyCollection<Guid> producerIds, CancellationToken cancellationToken);

    Task<Dictionary<string, CraftHeritageEntry>> GetCraftHeritageBySlugAsync(IReadOnlyCollection<string> slugs, CancellationToken cancellationToken);

    void RemoveState(ProductIndexState state);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
