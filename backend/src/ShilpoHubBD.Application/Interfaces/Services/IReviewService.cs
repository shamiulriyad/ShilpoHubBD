using ShilpoHubBD.Application.DTOs.Common;
using ShilpoHubBD.Application.DTOs.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Services;

public interface IReviewService
{
    Task<PagedResult<ReviewDto>> GetByProductAsync(Guid productId, ReviewQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<ReviewDto>> GetByHeritagePlaceAsync(Guid heritagePlaceId, ReviewQueryParameters query, CancellationToken cancellationToken);
    Task<PagedResult<ReviewDto>> GetByServiceAsync(Guid touristServiceId, ReviewQueryParameters query, CancellationToken cancellationToken);
    Task<ReviewDto> CreateAsync(Guid userId, CreateReviewRequest request, CancellationToken cancellationToken);
    Task<ReviewDto> UpdateAsync(Guid id, Guid userId, UpdateReviewRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, Guid userId, bool isAdmin, CancellationToken cancellationToken);
}
