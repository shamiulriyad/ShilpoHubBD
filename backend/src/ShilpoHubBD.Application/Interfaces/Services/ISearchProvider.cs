using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Application.Interfaces.Services;

// Abstraction over how search is executed. The current implementation runs native PostgreSQL
// full-text search; swap in an AI/embedding-based implementation later by registering a different
// ISearchProvider -- no changes needed in SearchService.
public interface ISearchProvider
{
    string Name { get; }

    Task<(List<Product> Items, int TotalCount)> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken);
}
