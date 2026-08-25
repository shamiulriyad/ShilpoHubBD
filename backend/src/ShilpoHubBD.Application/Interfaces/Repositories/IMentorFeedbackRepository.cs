using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IMentorFeedbackRepository
{
    Task<List<MentorFeedback>> GetByLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken);
    Task<List<MentorFeedback>> GetByMentorProfileAsync(Guid mentorProfileId, Guid learnerUserId, CancellationToken cancellationToken);
    Task AddAsync(MentorFeedback feedback, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
