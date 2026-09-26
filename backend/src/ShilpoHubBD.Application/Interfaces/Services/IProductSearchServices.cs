using ShilpoHubBD.Application.DTOs.ProductSearch;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IProductLookupService
{
    Task<List<LookupItemDto>> GetTypesAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<LookupItemDto> CreateTypeAsync(SaveLookupItemRequest request, CancellationToken cancellationToken);
    Task<LookupItemDto> UpdateTypeAsync(Guid id, SaveLookupItemRequest request, CancellationToken cancellationToken);
    Task DeleteTypeAsync(Guid id, CancellationToken cancellationToken);

    Task<List<LookupItemDto>> GetMaterialsAsync(bool includeInactive, CancellationToken cancellationToken);
    Task<LookupItemDto> CreateMaterialAsync(SaveLookupItemRequest request, CancellationToken cancellationToken);
    Task<LookupItemDto> UpdateMaterialAsync(Guid id, SaveLookupItemRequest request, CancellationToken cancellationToken);
    Task DeleteMaterialAsync(Guid id, CancellationToken cancellationToken);
}

public interface IProductAttributesService
{
    Task<ProductAttributesDto> GetAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProductAttributesDto> SaveAsync(Guid productId, SaveProductAttributesRequest request, Guid userId, bool isAdmin, CancellationToken cancellationToken);

    Task<AttributeSuggestionDto?> GetPendingSuggestionAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task<ProductAttributesDto> ConfirmSuggestionAsync(Guid productId, Guid suggestionId, ConfirmAttributeSuggestionRequest request, Guid userId, bool isAdmin, CancellationToken cancellationToken);
    Task DismissSuggestionAsync(Guid productId, Guid suggestionId, Guid userId, bool isAdmin, CancellationToken cancellationToken);

    /// <summary>Producer asks the AI for suggestions; the result is stored as a PENDING suggestion and changes nothing until confirmed.</summary>
    Task<AttributeSuggestionDto> GenerateSuggestionAsync(Guid productId, Guid userId, bool isAdmin, CancellationToken cancellationToken);

    /// <summary>Internal: store an AI proposal as a pending suggestion. Never touches the product's final attributes.</summary>
    Task<AttributeSuggestionDto> SubmitSuggestionAsync(SubmitAttributeSuggestionRequest request, CancellationToken cancellationToken);
}

public interface IProductSearchService
{
    Task<ProductSearchResultDto> SearchAsync(ProductSearchQuery query, CancellationToken cancellationToken);
}

/// <summary>The AI product-search service (Python): understands the question and returns candidate product ids. Null = unavailable.</summary>
public interface IProductSearchCandidateProvider
{
    Task<ProductSearchCandidatesDto?> GetCandidatesAsync(string query, int limit, CancellationToken cancellationToken);
}

/// <summary>The AI service that proposes product attributes from the product's text. Null = unavailable.</summary>
public interface IProductAttributeSuggester
{
    Task<GeneratedAttributeSuggestion?> SuggestAsync(ProductForSuggestion product, CancellationToken cancellationToken);
}

public interface IProductIndexService
{
    Task<ProductIndexBatchDto> GetPendingAsync(int limit, CancellationToken cancellationToken);
    Task AckAsync(ProductIndexAckRequest request, CancellationToken cancellationToken);
    Task<int> RequeueAllAsync(CancellationToken cancellationToken);
    Task<Dictionary<string, int>> GetStatsAsync(CancellationToken cancellationToken);
}
