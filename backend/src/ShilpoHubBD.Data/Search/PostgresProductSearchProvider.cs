using Microsoft.EntityFrameworkCore;
using NpgsqlTypes;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Search;

// PostgreSQL-backed implementation of ISearchProvider using native full-text search over the indexed, generated
// Products.SearchVector column (name + description + story), with an ILIKE fallback for partial/substring matches
// that full-text stemming would otherwise miss. No AI/embedding integration (see rag/products for AI search).
//
// Only public products are searchable: active AND admin-approved, exactly like the storefront listing.
public class PostgresProductSearchProvider : ISearchProvider
{
    private readonly ShilpoHubDbContext _context;

    public PostgresProductSearchProvider(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public string Name { get; } = "PostgresFullTextSearch";

    public async Task<(List<Product> Items, int TotalCount)> SearchAsync(string query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var likePattern = $"%{query}%";

        // The tsquery must be built INSIDE the LINQ expression so EF translates it to SQL; calling
        // PlainToTsQuery on a local variable throws "switched to client-evaluation" at runtime.
        var matches = _context.Products
            .Include(p => p.Category)
            .Include(p => p.District)
            .Include(p => p.Producer)
            .Include(p => p.Images)
            .Where(p => p.IsActive && p.ApprovalStatus == ProductApprovalStatus.Approved)
            .Where(p =>
                EF.Property<NpgsqlTsVector>(p, "SearchVector").Matches(EF.Functions.PlainToTsQuery("english", query)) ||
                EF.Functions.ILike(p.Name, likePattern) ||
                EF.Functions.ILike(p.Description, likePattern));

        var totalCount = await matches.CountAsync(cancellationToken);

        var items = await matches
            .OrderByDescending(p => EF.Property<NpgsqlTsVector>(p, "SearchVector").Rank(EF.Functions.PlainToTsQuery("english", query)))
            .ThenByDescending(p => p.BayesianRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
