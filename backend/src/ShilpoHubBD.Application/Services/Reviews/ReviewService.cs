using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Reviews;
using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Domain.Entities.TouristBooking;

namespace ShilpoHubBD.Application.Services.Reviews;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IHeritagePlaceRepository _heritagePlaceRepository;
    private readonly IHeritageCheckInRepository _checkInRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly ITouristServiceRepository _touristServiceRepository;

    public ReviewService(
        IReviewRepository reviewRepository,
        IProductRepository productRepository,
        IOrderRepository orderRepository,
        IHeritagePlaceRepository heritagePlaceRepository,
        IHeritageCheckInRepository checkInRepository,
        IBookingRepository bookingRepository,
        ITouristServiceRepository touristServiceRepository)
    {
        _reviewRepository = reviewRepository;
        _productRepository = productRepository;
        _orderRepository = orderRepository;
        _heritagePlaceRepository = heritagePlaceRepository;
        _checkInRepository = checkInRepository;
        _bookingRepository = bookingRepository;
        _touristServiceRepository = touristServiceRepository;
    }

    public async Task<PagedResult<ReviewDto>> GetByProductAsync(Guid productId, ReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _reviewRepository.GetPagedByProductAsync(productId, query.Page, query.PageSize, cancellationToken);
        return ToPagedResult(items, totalCount, query);
    }

    public async Task<PagedResult<ReviewDto>> GetByHeritagePlaceAsync(Guid heritagePlaceId, ReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _reviewRepository.GetPagedByHeritagePlaceAsync(heritagePlaceId, query.Page, query.PageSize, cancellationToken);
        return ToPagedResult(items, totalCount, query);
    }

    public async Task<PagedResult<ReviewDto>> GetByServiceAsync(Guid touristServiceId, ReviewQueryParameters query, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _reviewRepository.GetPagedByServiceAsync(touristServiceId, query.Page, query.PageSize, cancellationToken);
        return ToPagedResult(items, totalCount, query);
    }

    public async Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        if (request.ProductId.HasValue)
        {
            return await CreateProductReviewAsync(userId, request, cancellationToken);
        }

        if (request.HeritagePlaceId.HasValue)
        {
            return await CreateHeritagePlaceReviewAsync(userId, request, cancellationToken);
        }

        if (request.BookingId.HasValue)
        {
            return await CreateBookingReviewAsync(userId, request, cancellationToken);
        }

        throw new ConflictException("Exactly one of ProductId, HeritagePlaceId or BookingId must be set.");
    }

    private async Task<ReviewDto> CreateProductReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var productId = request.ProductId!.Value;

        if (await _productRepository.GetByIdAsync(productId, cancellationToken) is null)
        {
            throw new NotFoundException("Product not found.");
        }

        if (!await _orderRepository.HasPurchasedProductAsync(userId, productId, cancellationToken))
        {
            throw new ConflictException("You can only review products you have purchased and received.");
        }

        if (await _reviewRepository.GetByProductAndUserAsync(productId, userId, cancellationToken) is not null)
        {
            throw new ConflictException("You have already reviewed this product. Edit your existing review instead.");
        }

        var review = NewReview(userId, request);
        review.ProductId = productId;

        await _reviewRepository.AddAsync(review, cancellationToken);
        await AttachImagesAsync(review.Id, request.ImageUrls, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        await RecalculateProductRatingAsync(productId, cancellationToken);

        var created = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return ToDto(created!);
    }

    private async Task<ReviewDto> CreateHeritagePlaceReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var heritagePlaceId = request.HeritagePlaceId!.Value;

        if (await _heritagePlaceRepository.GetByIdAsync(heritagePlaceId, cancellationToken) is null)
        {
            throw new NotFoundException("Heritage place not found.");
        }

        if (!await _checkInRepository.HasCheckedInAsync(userId, heritagePlaceId, cancellationToken))
        {
            throw new ConflictException("You can only review heritage places you have checked in to.");
        }

        if (await _reviewRepository.GetByHeritagePlaceAndUserAsync(heritagePlaceId, userId, cancellationToken) is not null)
        {
            throw new ConflictException("You have already reviewed this heritage place. Edit your existing review instead.");
        }

        var review = NewReview(userId, request);
        review.HeritagePlaceId = heritagePlaceId;

        await _reviewRepository.AddAsync(review, cancellationToken);
        await AttachImagesAsync(review.Id, request.ImageUrls, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        await RecalculateHeritagePlaceRatingAsync(heritagePlaceId, cancellationToken);

        var created = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return ToDto(created!);
    }

    private async Task<ReviewDto> CreateBookingReviewAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken)
    {
        var bookingId = request.BookingId!.Value;

        var booking = await _bookingRepository.GetByIdAsync(bookingId, cancellationToken)
            ?? throw new NotFoundException("Booking not found.");

        if (booking.TouristId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to review this booking.");
        }

        if (booking.Status != BookingStatus.Completed)
        {
            throw new ConflictException("You can only review completed bookings.");
        }

        if (await _reviewRepository.GetByBookingAndUserAsync(bookingId, userId, cancellationToken) is not null)
        {
            throw new ConflictException("You have already reviewed this booking. Edit your existing review instead.");
        }

        var review = NewReview(userId, request);
        review.BookingId = bookingId;

        await _reviewRepository.AddAsync(review, cancellationToken);
        await AttachImagesAsync(review.Id, request.ImageUrls, cancellationToken);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        await RecalculateServiceRatingAsync(booking.ServiceId, cancellationToken);

        var created = await _reviewRepository.GetByIdAsync(review.Id, cancellationToken);
        return ToDto(created!);
    }

    private static Review NewReview(Guid userId, CreateReviewRequest request)
    {
        var now = DateTime.UtcNow;
        return new Review
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
        };
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

        await RecalculateRatingForSubjectAsync(review, cancellationToken);

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
        var heritagePlaceId = review.HeritagePlaceId;
        Guid? serviceId = null;
        if (review.BookingId.HasValue)
        {
            serviceId = (await _bookingRepository.GetByIdAsync(review.BookingId.Value, cancellationToken))?.ServiceId;
        }

        _reviewRepository.Remove(review);
        await _reviewRepository.SaveChangesAsync(cancellationToken);

        if (productId.HasValue)
        {
            await RecalculateProductRatingAsync(productId.Value, cancellationToken);
        }
        else if (heritagePlaceId.HasValue)
        {
            await RecalculateHeritagePlaceRatingAsync(heritagePlaceId.Value, cancellationToken);
        }
        else if (serviceId.HasValue)
        {
            await RecalculateServiceRatingAsync(serviceId.Value, cancellationToken);
        }
    }

    private async Task RecalculateRatingForSubjectAsync(Review review, CancellationToken cancellationToken)
    {
        if (review.ProductId.HasValue)
        {
            await RecalculateProductRatingAsync(review.ProductId.Value, cancellationToken);
        }
        else if (review.HeritagePlaceId.HasValue)
        {
            await RecalculateHeritagePlaceRatingAsync(review.HeritagePlaceId.Value, cancellationToken);
        }
        else if (review.BookingId.HasValue)
        {
            var booking = await _bookingRepository.GetByIdAsync(review.BookingId.Value, cancellationToken);
            if (booking is not null)
            {
                await RecalculateServiceRatingAsync(booking.ServiceId, cancellationToken);
            }
        }
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

    private async Task RecalculateHeritagePlaceRatingAsync(Guid heritagePlaceId, CancellationToken cancellationToken)
    {
        var (average, count) = await _reviewRepository.GetAggregateByHeritagePlaceAsync(heritagePlaceId, cancellationToken);

        var place = await _heritagePlaceRepository.GetByIdAsync(heritagePlaceId, cancellationToken);
        if (place is null)
        {
            return;
        }

        place.AverageRating = count == 0 ? 0 : Math.Round((decimal)average, 2);
        place.ReviewCount = count;
        await _heritagePlaceRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task RecalculateServiceRatingAsync(Guid touristServiceId, CancellationToken cancellationToken)
    {
        var (average, count) = await _reviewRepository.GetAggregateByServiceAsync(touristServiceId, cancellationToken);

        var service = await _touristServiceRepository.GetByIdAsync(touristServiceId, cancellationToken);
        if (service is null)
        {
            return;
        }

        service.AverageRating = count == 0 ? 0 : Math.Round((decimal)average, 2);
        service.ReviewCount = count;
        await _touristServiceRepository.SaveChangesAsync(cancellationToken);
    }

    private static PagedResult<ReviewDto> ToPagedResult(List<Review> items, int totalCount, ReviewQueryParameters query) => new()
    {
        Items = items.Select(ToDto).ToList(),
        TotalCount = totalCount,
        Page = query.Page,
        PageSize = query.PageSize,
    };

    private static ReviewDto ToDto(Review review) => new()
    {
        Id = review.Id,
        ProductId = review.ProductId,
        HeritagePlaceId = review.HeritagePlaceId,
        BookingId = review.BookingId,
        UserId = review.UserId,
        ReviewerName = review.User.FullName,
        Rating = review.Rating,
        Comment = review.Comment,
        ImageUrls = review.Images.OrderBy(i => i.DisplayOrder).Select(i => i.ImageUrl).ToList(),
        CreatedAt = review.CreatedAt,
        UpdatedAt = review.UpdatedAt,
    };
}
