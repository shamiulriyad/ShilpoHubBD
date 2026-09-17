using ShilpoHubBD.Application.Common;
using ShilpoHubBD.Application.DTOs.Cms;
using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Cms;

namespace ShilpoHubBD.Application.Services.Cms;

public class BlogPostService : IBlogPostService
{
    private readonly IBlogPostRepository _repository;
    private readonly IUserRepository _userRepository;

    public BlogPostService(IBlogPostRepository repository, IUserRepository userRepository)
    {
        _repository = repository;
        _userRepository = userRepository;
    }

    public async Task<PagedResult<BlogPostListItemDto>> GetPagedAsync(BlogPostQueryParameters query, CancellationToken cancellationToken)
    {
        query.Page = query.Page < 1 ? 1 : query.Page;
        query.PageSize = query.PageSize is < 1 or > 100 ? 12 : query.PageSize;

        var (items, totalCount) = await _repository.GetPagedAsync(query, publishedOnly: true, cancellationToken);
        return new PagedResult<BlogPostListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<PagedResult<BlogPostListItemDto>> GetDraftsAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize is < 1 or > 100 ? 12 : pageSize;

        var (items, totalCount) = await _repository.GetDraftsAsync(page, pageSize, cancellationToken);
        return new PagedResult<BlogPostListItemDto>
        {
            Items = items.Select(ToListItemDto).ToList(),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public async Task<BlogPostDto> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Blog post not found.");
        return ToDto(post);
    }

    public async Task<BlogPostDto> GetBySlugAsync(string slug, CancellationToken cancellationToken)
    {
        var post = await _repository.GetBySlugAsync(slug, cancellationToken)
            ?? throw new NotFoundException("Blog post not found.");
        return ToDto(post);
    }

    public async Task<BlogPostDto> CreateAsync(Guid authorUserId, CreateBlogPostRequest request, CancellationToken cancellationToken)
    {
        var author = await _userRepository.GetByIdAsync(authorUserId, cancellationToken)
            ?? throw new NotFoundException("Author not found.");

        var slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        var now = DateTime.UtcNow;

        var post = new BlogPost
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Slug = slug,
            Summary = request.Summary.Trim(),
            Content = request.Content.Trim(),
            CoverImageUrl = request.CoverImageUrl?.Trim(),
            Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim(),
            AuthorUserId = authorUserId,
            Author = author,
            IsPublished = request.Publish,
            PublishedAt = request.Publish ? now : null,
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _repository.AddAsync(post, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return ToDto(post);
    }

    public async Task<BlogPostDto> UpdateAsync(Guid id, UpdateBlogPostRequest request, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Blog post not found.");

        if (!post.Title.Equals(request.Title.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            post.Slug = await GenerateUniqueSlugAsync(request.Title, cancellationToken);
        }

        post.Title = request.Title.Trim();
        post.Summary = request.Summary.Trim();
        post.Content = request.Content.Trim();
        post.CoverImageUrl = request.CoverImageUrl?.Trim();
        post.Tags = string.IsNullOrWhiteSpace(request.Tags) ? null : request.Tags.Trim();

        if (request.IsPublished && !post.IsPublished)
        {
            post.PublishedAt = DateTime.UtcNow;
        }

        post.IsPublished = request.IsPublished;
        post.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync(cancellationToken);
        return ToDto(post);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var post = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Blog post not found.");

        _repository.Remove(post);
        await _repository.SaveChangesAsync(cancellationToken);
    }

    private async Task<string> GenerateUniqueSlugAsync(string title, CancellationToken cancellationToken)
    {
        var baseSlug = SlugGenerator.Generate(title);
        var slug = baseSlug;
        var suffix = 2;

        while (await _repository.ExistsBySlugAsync(slug, cancellationToken))
        {
            slug = $"{baseSlug}-{suffix++}";
        }

        return slug;
    }

    private static BlogPostListItemDto ToListItemDto(BlogPost post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Summary = post.Summary,
        CoverImageUrl = post.CoverImageUrl,
        Tags = post.Tags,
        AuthorName = post.Author.FullName,
        IsPublished = post.IsPublished,
        PublishedAt = post.PublishedAt,
    };

    private static BlogPostDto ToDto(BlogPost post) => new()
    {
        Id = post.Id,
        Title = post.Title,
        Slug = post.Slug,
        Summary = post.Summary,
        Content = post.Content,
        CoverImageUrl = post.CoverImageUrl,
        Tags = post.Tags,
        AuthorUserId = post.AuthorUserId,
        AuthorName = post.Author.FullName,
        IsPublished = post.IsPublished,
        PublishedAt = post.PublishedAt,
        CreatedAt = post.CreatedAt,
        UpdatedAt = post.UpdatedAt,
    };
}
