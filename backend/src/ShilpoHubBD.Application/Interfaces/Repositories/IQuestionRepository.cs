using ShilpoHubBD.Domain.Entities.Community;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IQuestionRepository
{
    Task<(List<CommunityQuestion> Items, int TotalCount)> GetPagedByProductAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken);
    Task<CommunityQuestion?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(CommunityQuestion question, CancellationToken cancellationToken);
    Task AddAnswerAsync(CommunityAnswer answer, CancellationToken cancellationToken);
    void RemoveAnswer(CommunityAnswer answer);
    void Remove(CommunityQuestion question);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
