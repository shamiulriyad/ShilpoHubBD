using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.LiveClass;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.LiveClass;

namespace ShilpoHubBD.Data.Repositories;

public class LiveClassRepository : ILiveClassRepository
{
    private readonly ShilpoHubDbContext _context;

    public LiveClassRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    private IQueryable<LiveClass> WithDetails()
        => _context.LiveClasses
            .Include(c => c.Instructor)
            .Include(c => c.Course)
            .Include(c => c.Participants).ThenInclude(p => p.User)
            .Include(c => c.Questions).ThenInclude(q => q.User)
            .Include(c => c.Attendances).ThenInclude(a => a.User)
            .AsSplitQuery();

    public Task<LiveClass?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => WithDetails().FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<(List<LiveClass> Items, int TotalCount)> GetPagedAsync(LiveClassQueryParameters query, CancellationToken cancellationToken)
    {
        var liveClasses = WithDetails().AsQueryable();

        if (query.InstructorUserId.HasValue)
        {
            liveClasses = liveClasses.Where(c => c.InstructorUserId == query.InstructorUserId.Value);
        }

        if (query.Status.HasValue)
        {
            liveClasses = liveClasses.Where(c => c.Status == query.Status.Value);
        }

        liveClasses = liveClasses.OrderByDescending(c => c.ScheduledStartAt);

        var totalCount = await liveClasses.CountAsync(cancellationToken);
        var items = await liveClasses
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public Task<List<LiveClass>> GetByInstructorAsync(Guid instructorUserId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(c => c.InstructorUserId == instructorUserId)
            .OrderByDescending(c => c.ScheduledStartAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(LiveClass liveClass, CancellationToken cancellationToken)
        => await _context.LiveClasses.AddAsync(liveClass, cancellationToken);

    public void Remove(LiveClass liveClass)
        => _context.LiveClasses.Remove(liveClass);

    public Task<LiveClassParticipant?> GetParticipantAsync(Guid liveClassId, Guid userId, CancellationToken cancellationToken)
        => _context.LiveClassParticipants
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.LiveClassId == liveClassId && p.UserId == userId, cancellationToken);

    public async Task AddParticipantAsync(LiveClassParticipant participant, CancellationToken cancellationToken)
        => await _context.LiveClassParticipants.AddAsync(participant, cancellationToken);

    public Task<List<LiveClass>> GetMyRegisteredAsync(Guid userId, CancellationToken cancellationToken)
        => WithDetails()
            .Where(c => c.Participants.Any(p => p.UserId == userId))
            .OrderByDescending(c => c.ScheduledStartAt)
            .ToListAsync(cancellationToken);

    public Task<LiveClassQuestion?> GetQuestionByIdAsync(Guid questionId, CancellationToken cancellationToken)
        => _context.LiveClassQuestions
            .Include(q => q.LiveClass)
            .Include(q => q.User)
            .FirstOrDefaultAsync(q => q.Id == questionId, cancellationToken);

    public async Task AddQuestionAsync(LiveClassQuestion question, CancellationToken cancellationToken)
        => await _context.LiveClassQuestions.AddAsync(question, cancellationToken);

    public Task<LiveClassAttendance?> GetOpenAttendanceAsync(Guid liveClassId, Guid userId, CancellationToken cancellationToken)
        => _context.LiveClassAttendances
            .Where(a => a.LiveClassId == liveClassId && a.UserId == userId && a.LeftAt == null)
            .OrderByDescending(a => a.JoinedAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task AddAttendanceAsync(LiveClassAttendance attendance, CancellationToken cancellationToken)
        => await _context.LiveClassAttendances.AddAsync(attendance, cancellationToken);

    public Task<List<LiveClassAttendance>> GetAttendanceAsync(Guid liveClassId, CancellationToken cancellationToken)
        => _context.LiveClassAttendances
            .Include(a => a.User)
            .Where(a => a.LiveClassId == liveClassId)
            .OrderByDescending(a => a.JoinedAt)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
