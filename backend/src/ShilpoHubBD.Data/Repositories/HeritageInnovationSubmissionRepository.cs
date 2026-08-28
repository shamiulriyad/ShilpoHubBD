using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Innovation;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Innovation;

namespace ShilpoHubBD.Data.Repositories;

public class HeritageInnovationSubmissionRepository : IHeritageInnovationSubmissionRepository
{
    private readonly ShilpoHubDbContext _context;

    public HeritageInnovationSubmissionRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    public Task<HeritageInnovationSubmission?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.HeritageInnovationSubmissions
            .Include(s => s.Submitter)
            .Include(s => s.TeamMembers)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public Task<HeritageInnovationSubmission?> GetDetailAsync(Guid id, CancellationToken cancellationToken)
        => _context.HeritageInnovationSubmissions
            .Include(s => s.Submitter)
            .Include(s => s.DecisionBy)
            .Include(s => s.TeamMembers).ThenInclude(m => m.User)
            .Include(s => s.Reviews).ThenInclude(r => r.Reviewer)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<(List<HeritageInnovationSubmission> Items, int TotalCount)> GetPagedAsync(
        Guid userId, bool isReviewer, HeritageInnovationSubmissionQueryParameters query, CancellationToken cancellationToken)
    {
        var submissions = _context.HeritageInnovationSubmissions
            .Include(s => s.Submitter)
            .Include(s => s.TeamMembers)
            .Include(s => s.Reviews)
            .AsSplitQuery()
            .AsQueryable();

        var scope = query.Scope?.Trim().ToLowerInvariant();
        submissions = scope switch
        {
            "mine" => submissions.Where(s => s.SubmitterUserId == userId
                || s.TeamMembers.Any(m => m.UserId == userId)),
            "review" when isReviewer => submissions.Where(s =>
                s.Status == InnovationSubmissionStatus.Submitted || s.Status == InnovationSubmissionStatus.UnderReview),
            _ => isReviewer
                ? submissions.Where(s => s.SubmitterUserId == userId
                    || s.TeamMembers.Any(m => m.UserId == userId)
                    || s.Status == InnovationSubmissionStatus.Submitted
                    || s.Status == InnovationSubmissionStatus.UnderReview
                    || s.Status == InnovationSubmissionStatus.Approved
                    || s.Status == InnovationSubmissionStatus.Rejected)
                : submissions.Where(s => s.SubmitterUserId == userId
                    || s.TeamMembers.Any(m => m.UserId == userId)),
        };

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<InnovationSubmissionStatus>(query.Status, true, out var status))
        {
            submissions = submissions.Where(s => s.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            submissions = submissions.Where(s => s.Title.ToLower().Contains(term) || s.Problem.ToLower().Contains(term));
        }

        submissions = submissions.OrderByDescending(s => s.UpdatedAt);

        var totalCount = await submissions.CountAsync(cancellationToken);
        var items = await submissions
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddAsync(HeritageInnovationSubmission submission, CancellationToken cancellationToken)
        => await _context.HeritageInnovationSubmissions.AddAsync(submission, cancellationToken);

    public void Remove(HeritageInnovationSubmission submission)
        => _context.HeritageInnovationSubmissions.Remove(submission);

    public Task<SubmissionTeamMember?> GetTeamMemberByIdAsync(Guid memberId, CancellationToken cancellationToken)
        => _context.SubmissionTeamMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

    public async Task AddTeamMemberAsync(SubmissionTeamMember member, CancellationToken cancellationToken)
        => await _context.SubmissionTeamMembers.AddAsync(member, cancellationToken);

    public void RemoveTeamMember(SubmissionTeamMember member)
        => _context.SubmissionTeamMembers.Remove(member);

    public async Task AddReviewAsync(SubmissionReview review, CancellationToken cancellationToken)
        => await _context.SubmissionReviews.AddAsync(review, cancellationToken);

    public Task<List<SubmissionReview>> GetReviewsAsync(Guid submissionId, CancellationToken cancellationToken)
        => _context.SubmissionReviews
            .Include(r => r.Reviewer)
            .Where(r => r.HeritageInnovationSubmissionId == submissionId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddEventAsync(SubmissionEvent submissionEvent, CancellationToken cancellationToken)
        => await _context.SubmissionEvents.AddAsync(submissionEvent, cancellationToken);

    public Task<List<SubmissionEvent>> GetHistoryAsync(Guid submissionId, int take, CancellationToken cancellationToken)
        => _context.SubmissionEvents
            .Include(e => e.Actor)
            .Where(e => e.HeritageInnovationSubmissionId == submissionId)
            .OrderByDescending(e => e.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
