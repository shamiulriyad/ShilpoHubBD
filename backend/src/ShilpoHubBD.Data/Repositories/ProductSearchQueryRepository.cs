using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.ProductSearch;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Marketplace;

namespace ShilpoHubBD.Data.Repositories;

public class ProductSearchQueryRepository : IProductSearchQueryRepository
{
    private const int CandidateCap = 300;

    private readonly ShilpoHubDbContext _context;

    public ProductSearchQueryRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Product> Items, int Total)> SearchAsync(ProductSearchCriteria c, int? skip, int? take, CancellationToken cancellationToken)
    {
        // Visibility is not a filter the AI can influence: only active, admin-approved products are ever returned.
        var query = _context.Products.AsNoTracking()
            .Where(p => p.IsActive && p.ApprovalStatus == ProductApprovalStatus.Approved);

        if (c.Ids is not null) query = query.Where(p => c.Ids.Contains(p.Id));
        if (c.MinPrice is { } min) query = query.Where(p => p.EffectivePrice >= min);
        if (c.MaxPrice is { } max) query = query.Where(p => p.EffectivePrice <= max);
        if (c.MinRating is { } rating) query = query.Where(p => p.ReviewCount > 0 && p.AverageRating >= rating);
        if (c.InStockOnly) query = query.Where(p => p.Stock > 0 || p.Variants.Any(v => v.IsActive && v.Stock > 0));
        if (!string.IsNullOrWhiteSpace(c.District)) query = query.Where(p => EF.Functions.ILike(p.District.Name, c.District));
        else if (!string.IsNullOrWhiteSpace(c.Division)) query = query.Where(p => EF.Functions.ILike(p.District.Division, c.Division));
        if (!string.IsNullOrWhiteSpace(c.CategorySlug)) query = query.Where(p => p.Category.Slug == c.CategorySlug);
        if (!string.IsNullOrWhiteSpace(c.ProductTypeSlug)) query = query.Where(p => p.ProductType != null && p.ProductType.Slug == c.ProductTypeSlug);
        if (c.MaterialSlugs.Count > 0) query = query.Where(p => p.Materials.Any(m => c.MaterialSlugs.Contains(m.Material.Slug)));

        if (c.Keywords.Count > 0)
        {
            // Keyword search is only the fallback when the AI service is down, so any keyword may match.
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(Product), "p");
            System.Linq.Expressions.Expression? any = null;
            foreach (var keyword in c.Keywords)
            {
                var pattern = $"%{keyword}%";
                System.Linq.Expressions.Expression<Func<Product, bool>> match =
                    p => EF.Functions.ILike(p.Name, pattern) || EF.Functions.ILike(p.Description, pattern) || EF.Functions.ILike(p.Category.Name, pattern);
                var body = new ParameterReplacer(parameter).Visit(match.Body)!;
                any = any is null ? body : System.Linq.Expressions.Expression.OrElse(any, body);
            }

            query = query.Where(System.Linq.Expressions.Expression.Lambda<Func<Product, bool>>(any!, parameter));
        }

        var total = await query.CountAsync(cancellationToken);

        var ordered = c.Sort switch
        {
            "rating" => query.OrderByDescending(p => p.BayesianRating).ThenByDescending(p => p.ReviewCount),
            "price_asc" => query.OrderBy(p => p.EffectivePrice),
            "price_desc" => query.OrderByDescending(p => p.EffectivePrice),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "popular" => query.OrderByDescending(p => p.SalesCount),
            _ => query.OrderByDescending(p => p.BayesianRating).ThenByDescending(p => p.SalesCount),
        };

        var paged = take is null ? ordered.Take(CandidateCap) : ordered.Skip(skip ?? 0).Take(take.Value);
        var items = await paged
            .Include(p => p.Category)
            .Include(p => p.District)
            .Include(p => p.Producer)
            .Include(p => p.ProductType)
            .Include(p => p.Attributes)
            .Include(p => p.Materials).ThenInclude(m => m.Material)
            .Include(p => p.Images)
            .Include(p => p.Variants)
            .AsSplitQuery()
            .ToListAsync(cancellationToken);
        return (items, total);
    }
}

// Re-targets a lambda body onto a shared parameter so several per-keyword predicates can be OR-ed into one expression.
internal sealed class ParameterReplacer : System.Linq.Expressions.ExpressionVisitor
{
    private readonly System.Linq.Expressions.ParameterExpression _target;

    public ParameterReplacer(System.Linq.Expressions.ParameterExpression target) => _target = target;

    protected override System.Linq.Expressions.Expression VisitParameter(System.Linq.Expressions.ParameterExpression node)
        => node.Type == typeof(Product) ? _target : base.VisitParameter(node);
}
