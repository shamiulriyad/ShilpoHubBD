using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Mentorship;

namespace ShilpoHubBD.Data.Repositories;

public class MentorshipRequestRepository : IMentorshipRequestRepository
{
    private readonly ShilpoHubDbContext _context;

    public MentorshipRequestRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<MentorshipRequest> WithDetails()
        => _context.MentorshipRequests
            .Include(r => r.MentorProfile).ThenInclude(m => m.User)
            .Include(r => r.Learner)
            .Include(r => r.HeritageSkill)
            .AsSplitQuery();

    public Task<MentorshipRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(r => r.Id == id, cancellationToken);

    public Task<List<MentorshipRequest>> GetByLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(r => r.LearnerUserId == learnerUserId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);

    public Task<List<MentorshipRequest>> GetByMentorProfileAsync(Guid mentorProfileId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(r => r.MentorProfileId == mentorProfileId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync(cancellationToken);

    public Task<bool> HasOpenRequestAsync(Guid mentorProfileId, Guid learnerUserId, CancellationToken cancellationToken)
        => _context.MentorshipRequests.AnyAsync(
            r => r.MentorProfileId == mentorProfileId
                && r.LearnerUserId == learnerUserId
                && (r.Status == MentorshipRequestStatus.Pending || r.Status == MentorshipRequestStatus.Accepted),
            cancellationToken);

    public async Task AddAsync(MentorshipRequest request, CancellationToken cancellationToken)
        => await _context.MentorshipRequests.AddAsync(request, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
