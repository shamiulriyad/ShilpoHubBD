using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<(List<Review> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<(List<Review> Items, int TotalCount)> GetPagedByHeritagePlaceAsync(Guid heritagePlaceId, int page, int pageSize, CancellationToken cancellationToken);
    Task<(List<Review> Items, int TotalCount)> GetPagedByServiceAsync(Guid touristServiceId, int page, int pageSize, CancellationToken cancellationToken);

    Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Review?> GetByProductAndUserAsync(Guid productId, Guid userId, CancellationToken cancellationToken);
    Task<Review?> GetByHeritagePlaceAndUserAsync(Guid heritagePlaceId, Guid userId, CancellationToken cancellationToken);
    Task<Review?> GetByBookingAndUserAsync(Guid bookingId, Guid userId, CancellationToken cancellationToken);

    Task<(double AverageRating, int ReviewCount)> GetAggregateAsync(Guid productId, CancellationToken cancellationToken);
    Task<(double AverageRating, int ReviewCount)> GetAggregateByHeritagePlaceAsync(Guid heritagePlaceId, CancellationToken cancellationToken);
    Task<(double AverageRating, int ReviewCount)> GetAggregateByServiceAsync(Guid touristServiceId, CancellationToken cancellationToken);

    Task AddAsync(Review review, CancellationToken cancellationToken);
    Task AddImageAsync(ReviewImage image, CancellationToken cancellationToken);
    void RemoveImage(ReviewImage image);
    void Remove(Review review);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
