using ShilpoHubBD.Domain.Entities.Mentorship;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

public interface IMentorshipRequestRepository
{
    Task<MentorshipRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<MentorshipRequest>> GetByLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken);
    Task<List<MentorshipRequest>> GetByMentorProfileAsync(Guid mentorProfileId, CancellationToken cancellationToken);
    Task<bool> HasOpenRequestAsync(Guid mentorProfileId, Guid learnerUserId, CancellationToken cancellationToken);
    Task AddAsync(MentorshipRequest request, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
