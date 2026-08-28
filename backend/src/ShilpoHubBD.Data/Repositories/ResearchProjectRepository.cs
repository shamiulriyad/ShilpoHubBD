using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Data.Repositories;

public class ResearchProjectRepository : IResearchProjectRepository
{
    private readonly ShilpoHubDbContext _context;

    public ResearchProjectRepository(ShilpoHubDbContext context)
    {
        _context = context;
    }

    // ---- Projects ------------------------------------------------------------

    public Task<ResearchProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => _context.ResearchProjects
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<ResearchProject?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken)
        => _context.ResearchProjects
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken)
        => _context.ResearchProjects.AnyAsync(p => p.Slug == slug, cancellationToken);

    public async Task<(List<ResearchProject> Items, int TotalCount)> GetPagedForMemberAsync(
        Guid userId, ResearchProjectQueryParameters query, CancellationToken cancellationToken)
    {
        var projects = _context.ResearchProjects
            .Include(p => p.Owner)
            .Include(p => p.Members)
            .Include(p => p.Tasks)
            .AsSplitQuery()
            .Where(p => p.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<ResearchProjectStatus>(query.Status, true, out var status))
        {
            projects = projects.Where(p => p.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            projects = projects.Where(p =>
                p.Title.ToLower().Contains(term) || p.Summary.ToLower().Contains(term));
        }

        projects = projects.OrderByDescending(p => p.UpdatedAt);

        var totalCount = await projects.CountAsync(cancellationToken);
        var items = await projects
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<ResearchProjectStats> GetStatsAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var taskCounts = await _context.ResearchTasks
            .Where(t => t.ResearchProjectId == projectId)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Open = g.Count(t => t.Status != ResearchTaskStatus.Done && t.Status != ResearchTaskStatus.Cancelled),
            })
            .FirstOrDefaultAsync(cancellationToken);

        return new ResearchProjectStats
        {
            TaskCount = taskCounts?.Total ?? 0,
            OpenTaskCount = taskCounts?.Open ?? 0,
            MilestoneCount = await _context.ResearchMilestones.CountAsync(m => m.ResearchProjectId == projectId, cancellationToken),
            NoteCount = await _context.ResearchNotes.CountAsync(n => n.ResearchProjectId == projectId, cancellationToken),
            PaperCount = await _context.ResearchPapers.CountAsync(p => p.ResearchProjectId == projectId, cancellationToken),
            PublicationCount = await _context.ResearchPublications.CountAsync(p => p.ResearchProjectId == projectId, cancellationToken),
        };
    }

    public async Task AddAsync(ResearchProject project, CancellationToken cancellationToken)
        => await _context.ResearchProjects.AddAsync(project, cancellationToken);

    public void Remove(ResearchProject project)
        => _context.ResearchProjects.Remove(project);

    // ---- Members -----------------------------------------------------------

    public Task<ResearchProjectMember?> GetMembershipAsync(Guid projectId, Guid userId, CancellationToken cancellationToken)
        => _context.ResearchProjectMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.ResearchProjectId == projectId && m.UserId == userId, cancellationToken);

    public Task<ResearchProjectMember?> GetMemberByIdAsync(Guid memberId, CancellationToken cancellationToken)
        => _context.ResearchProjectMembers
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

    public Task<List<ResearchProjectMember>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken)
        => _context.ResearchProjectMembers
            .Include(m => m.User)
            .Where(m => m.ResearchProjectId == projectId)
            .OrderBy(m => m.JoinedAt)
            .ToListAsync(cancellationToken);

    public async Task AddMemberAsync(ResearchProjectMember member, CancellationToken cancellationToken)
        => await _context.ResearchProjectMembers.AddAsync(member, cancellationToken);

    public void RemoveMember(ResearchProjectMember member)
        => _context.ResearchProjectMembers.Remove(member);

    // ---- Tasks -----------------------------------------------------------

    public Task<ResearchTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken)
        => _context.ResearchTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Milestone)
            .FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);

    public async Task<(List<ResearchTask> Items, int TotalCount)> GetTasksAsync(
        Guid projectId, ResearchTaskQueryParameters query, CancellationToken cancellationToken)
    {
        var tasks = _context.ResearchTasks
            .Include(t => t.AssignedTo)
            .Include(t => t.CreatedBy)
            .Include(t => t.Milestone)
            .AsSplitQuery()
            .Where(t => t.ResearchProjectId == projectId);

        if (!string.IsNullOrWhiteSpace(query.Status)
            && Enum.TryParse<ResearchTaskStatus>(query.Status, true, out var status))
        {
            tasks = tasks.Where(t => t.Status == status);
        }

        if (query.AssignedToUserId.HasValue)
        {
            tasks = tasks.Where(t => t.AssignedToUserId == query.AssignedToUserId.Value);
        }

        if (query.MilestoneId.HasValue)
        {
            tasks = tasks.Where(t => t.MilestoneId == query.MilestoneId.Value);
        }

        tasks = tasks
            .OrderBy(t => t.Status == ResearchTaskStatus.Done || t.Status == ResearchTaskStatus.Cancelled)
            .ThenByDescending(t => t.Priority)
            .ThenBy(t => t.DueDate ?? DateTime.MaxValue)
            .ThenByDescending(t => t.CreatedAt);

        var totalCount = await tasks.CountAsync(cancellationToken);
        var items = await tasks
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddTaskAsync(ResearchTask task, CancellationToken cancellationToken)
        => await _context.ResearchTasks.AddAsync(task, cancellationToken);

    public void RemoveTask(ResearchTask task)
        => _context.ResearchTasks.Remove(task);

    // ---- Milestones -----------------------------------------------------------

    public Task<ResearchMilestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken)
        => _context.ResearchMilestones.FirstOrDefaultAsync(m => m.Id == milestoneId, cancellationToken);

    public Task<List<ResearchMilestone>> GetMilestonesAsync(Guid projectId, CancellationToken cancellationToken)
        => _context.ResearchMilestones
            .Where(m => m.ResearchProjectId == projectId)
            .OrderBy(m => m.OrderIndex)
            .ThenBy(m => m.TargetDate ?? DateTime.MaxValue)
            .ToListAsync(cancellationToken);

    public Task<int> CountTasksForMilestoneAsync(Guid milestoneId, CancellationToken cancellationToken)
        => _context.ResearchTasks.CountAsync(t => t.MilestoneId == milestoneId, cancellationToken);

    public async Task AddMilestoneAsync(ResearchMilestone milestone, CancellationToken cancellationToken)
        => await _context.ResearchMilestones.AddAsync(milestone, cancellationToken);

    public void RemoveMilestone(ResearchMilestone milestone)
        => _context.ResearchMilestones.Remove(milestone);

    // ---- Notes -----------------------------------------------------------

    public Task<ResearchNote?> GetNoteByIdAsync(Guid noteId, CancellationToken cancellationToken)
        => _context.ResearchNotes
            .Include(n => n.Author)
            .FirstOrDefaultAsync(n => n.Id == noteId, cancellationToken);

    public Task<List<ResearchNote>> GetNotesAsync(Guid projectId, CancellationToken cancellationToken)
        => _context.ResearchNotes
            .Include(n => n.Author)
            .Where(n => n.ResearchProjectId == projectId)
            .OrderByDescending(n => n.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddNoteAsync(ResearchNote note, CancellationToken cancellationToken)
        => await _context.ResearchNotes.AddAsync(note, cancellationToken);

    public void RemoveNote(ResearchNote note)
        => _context.ResearchNotes.Remove(note);

    // ---- Papers -----------------------------------------------------------

    public Task<ResearchPaper?> GetPaperByIdAsync(Guid paperId, CancellationToken cancellationToken)
        => _context.ResearchPapers
            .Include(p => p.CreatedBy)
            .FirstOrDefaultAsync(p => p.Id == paperId, cancellationToken);

    public Task<List<ResearchPaper>> GetPapersAsync(Guid projectId, CancellationToken cancellationToken)
        => _context.ResearchPapers
            .Include(p => p.CreatedBy)
            .Where(p => p.ResearchProjectId == projectId)
            .OrderByDescending(p => p.UpdatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddPaperAsync(ResearchPaper paper, CancellationToken cancellationToken)
        => await _context.ResearchPapers.AddAsync(paper, cancellationToken);

    public void RemovePaper(ResearchPaper paper)
        => _context.ResearchPapers.Remove(paper);

    // ---- Publications -----------------------------------------------------------

    public Task<ResearchPublication?> GetPublicationByIdAsync(Guid publicationId, CancellationToken cancellationToken)
        => _context.ResearchPublications
            .Include(p => p.CreatedBy)
            .Include(p => p.Project)
            .FirstOrDefaultAsync(p => p.Id == publicationId, cancellationToken);

    public Task<List<ResearchPublication>> GetPublicationsForProjectAsync(Guid projectId, CancellationToken cancellationToken)
        => _context.ResearchPublications
            .Include(p => p.CreatedBy)
            .Include(p => p.Project)
            .Where(p => p.ResearchProjectId == projectId)
            .OrderByDescending(p => p.PublishedOn ?? DateTime.MinValue)
            .ThenByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<(List<ResearchPublication> Items, int TotalCount)> GetPublicationRepositoryAsync(
        Guid userId, ResearchPublicationQueryParameters query, CancellationToken cancellationToken)
    {
        var publications = _context.ResearchPublications
            .Include(p => p.CreatedBy)
            .Include(p => p.Project)
            .AsSplitQuery()
            .Where(p => p.IsPublic || p.Project.Members.Any(m => m.UserId == userId));

        if (!string.IsNullOrWhiteSpace(query.Type)
            && Enum.TryParse<ResearchPublicationType>(query.Type, true, out var type))
        {
            publications = publications.Where(p => p.Type == type);
        }

        if (query.Year.HasValue)
        {
            publications = publications.Where(p => p.PublishedOn != null && p.PublishedOn.Value.Year == query.Year.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            publications = publications.Where(p =>
                p.Title.ToLower().Contains(term)
                || p.Authors.ToLower().Contains(term)
                || (p.Venue != null && p.Venue.ToLower().Contains(term)));
        }

        publications = publications
            .OrderByDescending(p => p.PublishedOn ?? DateTime.MinValue)
            .ThenByDescending(p => p.CreatedAt);

        var totalCount = await publications.CountAsync(cancellationToken);
        var items = await publications
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task AddPublicationAsync(ResearchPublication publication, CancellationToken cancellationToken)
        => await _context.ResearchPublications.AddAsync(publication, cancellationToken);

    public void RemovePublication(ResearchPublication publication)
        => _context.ResearchPublications.Remove(publication);

    // ---- Activity -----------------------------------------------------------

    public async Task AddActivityAsync(ResearchActivity activity, CancellationToken cancellationToken)
        => await _context.ResearchActivities.AddAsync(activity, cancellationToken);

    public Task<List<ResearchActivity>> GetActivitiesAsync(Guid projectId, int take, CancellationToken cancellationToken)
        => _context.ResearchActivities
            .Include(a => a.Actor)
            .Where(a => a.ResearchProjectId == projectId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(take)
            .ToListAsync(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _context.SaveChangesAsync(cancellationToken);
}
