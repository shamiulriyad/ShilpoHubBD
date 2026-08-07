using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Services.Reviews;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;

    public ReviewService(IReviewRepository reviewRepository, IProductRepository productRepository, IOrderRepository orderRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
    }

    public async Task<PagedResult<ReviewDto>> GetByProductAsync(Guid productId, ReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _reviewRepository.GetPagedByProductAsync(productId, query.Page, query.PageSize, cancellationToken);
        return new PagedResult<ReviewDto>
        {
            Items = items.Select(ToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize,
        };
    }

    public async Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (await _productRepository.GetByIdAsync(request.ProductId, cancellationToken) is null)
        {
            throw new NotFoundException("Product not found.");
        }

        if (!await _orderRepository.HasPurchasedProductAsync(userId, request.ProductId, cancellationToken))
        {
            throw new ConflictException("You can only review products you have purchased and received.");
        }

        if (await _reviewRepository.GetByProductAndUserAsync(request.ProductId, userId, cancellationToken) is not null)
        {
            throw new ConflictException("You have already reviewed this product. Edit your existing review instead.");
        }

        var now = DateTime.UtcNow;
        var review = new Review
        {
            Id = Guid.NewGuid(),
            ProductId = request.ProductId,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };

        await _reviewRepository.AddAsync(review, cancellationToken);
        await AttachImagesAsync(review.Id, request.ImageUrls, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        await RecalculateProductRatingAsync(request.ProductId, cancellationToken);

        var created = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return ToDto(created!);
    }

    public async Task<ReviewDto> UpdateAsync(Guid id, Guid userId, UpdateReviewRequest request, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

        if (review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to edit this review.");
        }

        review.Rating = request.Rating;
        review.Comment = request.Comment.Trim();
        review.UpdatedAt = DateTime.UtcNow;

        foreach (var image in review.Images.ToList())
        {
            _reviewRepository.RemoveImage(image);
        }

        await AttachImagesAsync(review.Id, request.ImageUrls, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        await RecalculateProductRatingAsync(review.ProductId, cancellationToken);

        var updated = await _reviewRepository.GetByIdAsync(id, cancellationToken);
        return ToDto(updated!);
    }

    public async Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken)
    {
        var review = await _reviewRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException("Review not found.");

        if (!isAdmin && review.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to delete this review.");
        }

        var productId = review.ProductId;
        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        await RecalculateProductRatingAsync(productId, cancellationToken);
    }

    private async Task AttachImagesAsync(Guid reviewId, List<string> imageUrls, CancellationToken cancellationToken)
    {
        for (var i = 0; i < imageUrls.Count; i++)
        {
            await _reviewRepository.AddImageAsync(new ReviewImage
            {
                Id = Guid.NewGuid(),
                ReviewId = reviewId,
                ImageUrl = imageUrls[i].Trim(),
                DisplayOrder = i,
            }, cancellationToken);
        }
    }

    private async Task RecalculateProductRatingAsync(Guid productId, CancellationToken cancellationToken)
    {
        var (average, count) = await _reviewRepository.GetAggregateAsync(productId, cancellationToken);

        var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
        if (product is null)
        {
            return;
        }

        product.AverageRating = count == 0 ? 0 : Math.Round((decimal)average, 2);
        product.ReviewCount = count;
        await _productRepository.SaveChangesAsync(cancellationToken);
    }

    private static ReviewDto ToDto(Review review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        UserId = review.UserId,
        ReviewerName = review.User.FullName,
        Rating = review.Rating,
        Comment = review.Comment,
        ImageUrls = review.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
        CreatedAt = review.CreatedAt,
        UpdatedAt = review.UpdatedAt,
    };
}
