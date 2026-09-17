using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Data.Repositories;

public class BlogPostRepository : IBlogPostRepository
{
    private readonly ShilpoHubDbContext _context;

    public BlogPostRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<BlogPost> WithDetails()
        => _context.BlogPosts.Include(b => b.Author);

    public async Task<(List<BlogPost> Items, int TotalCount)> GetPagedAsync(
        BlogPostQueryParameters query, bool publishedOnly, CancellationToken cancellationToken)
    {
        var posts = WithDetails();

        if (publishedOnly)
        {
            posts = posts.Where(b => b.IsPublished);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = $"%{query.Search.Trim()}%";
            posts = posts.Where(b => EF.Functions.ILike(b.Title, term) || EF.Functions.ILike(b.Summary, term));
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var term = $"%{query.Tag.Trim()}%";
            posts = posts.Where(b => b.Tags != null && EF.Functions.ILike(b.Tags, term));
        }

        posts = posts.OrderByDescending(b => b.PublishedAt ?? b.CreatedAt);

        var totalCount = await posts.CountAsync(cancellationToken);
        var items = await posts
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<BlogPost> Items, int TotalCount)> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        var posts = WithDetails().Where(b => !b.IsPublished).OrderByDescending(b => b.CreatedAt);

        var totalCount = await posts.CountAsync(cancellationToken);
        var items = await posts.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<BlogPost?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<BlogPost?> GetBySlugAsync(string slug, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(b => b.Slug == slug, cancellationToken);

    public Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken)
        => _context.BlogPosts.AnyAsync(b => b.Slug == slug, cancellationToken);

    public async Task AddAsync(BlogPost post, CancellationToken cancellationToken)
        => await _context.BlogPosts.AddAsync(post, cancellationToken);

    public void Remove(BlogPost post)
        => _context.BlogPosts.Remove(post);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
