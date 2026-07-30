using ShilpoHubBD.Domain.Entities.Reviews;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IReviewRepository
{
	Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}
