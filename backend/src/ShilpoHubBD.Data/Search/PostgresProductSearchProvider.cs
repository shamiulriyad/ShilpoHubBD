using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Search;

// PostgreSQL-backed implementation of ISearchProvider using native full-text search
// (to_tsvector / plainto_tsquery) with an ILIKE fallback for partial/substring matches
// that full-text stemming would otherwise miss. No AI/embedding integration.
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
        var tsQuery = EF.Functions.PlainToTsQuery("english", query);
        var likePattern = $"%{query}%";

        var matches = _context.Products
            .Include(p => p.Category)
            .Include(p => p.District)
            .Include(p => p.Producer)
            .Include(p => p.Images)
            .Where(p => p.IsActive)
            .Where(p =>
                EF.Functions.ToTsVector("english", p.Name + " " + p.Description).Matches(tsQuery) ||
                EF.Functions.ILike(p.Name, likePattern) ||
                EF.Functions.ILike(p.Description, likePattern));

        var totalCount = await matches.CountAsync(cancellationToken);

        var items = await matches
            .OrderByDescending(p => EF.Functions.ToTsVector("english", p.Name + " " + p.Description).Rank(tsQuery))
            .ThenByDescending(p => p.AverageRating)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
