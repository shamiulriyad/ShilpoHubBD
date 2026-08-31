using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Portfolio;

namespace ShilpoHubBD.Data.Repositories;

public class MentorFeedbackRepository : IMentorFeedbackRepository
{
    private readonly ShilpoHubDbContext _context;

    public MentorFeedbackRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<MentorFeedback> WithDetails()
        => _context.MentorFeedbacks
            .Include(f => f.MentorProfile).ThenInclude(m => m.User)
            .Include(f => f.HeritageSkill)
            .AsSplitQuery();

    public Task<List<MentorFeedback>> GetByLearnerAsync(Guid learnerUserId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(f => f.LearnerUserId == learnerUserId)
            .OrderByDescending(f => f.CreatedAt)
            .ToListAsync(cancellationToken);

    public Task<List<MentorFeedback>> GetByMentorProfileAsync(Guid mentorProfileId, Guid learnerUserId, CancellationToken cancellationToken)
        => _context.MentorFeedbacks
            .Where(f => f.MentorProfileId == mentorProfileId && f.LearnerUserId == learnerUserId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(MentorFeedback feedback, CancellationToken cancellationToken)
        => await _context.MentorFeedbacks.AddAsync(feedback, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
