using ShilpoHubBD.Application.DTOs.Research;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Interfaces.Repositories;

/// <summary>
/// Aggregate repository for the Research Workspace module. Covers the <see cref="ResearchProject"/>
/// root plus its members, tasks, milestones, notes, papers, publications and activity trail.
/// </summary>
public interface IResearchProjectRepository
{
    // Projects
    Task<ResearchProject?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<ResearchProject?> GetByIdWithMembersAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, CancellationToken cancellationToken);
    Task<(List<ResearchProject> Items, int TotalCount)> GetPagedForMemberAsync(
        Guid userId, ResearchProjectQueryParameters query, CancellationToken cancellationToken);
    Task<ResearchProjectStats> GetStatsAsync(Guid projectId, CancellationToken cancellationToken);
    Task AddAsync(ResearchProject project, CancellationToken cancellationToken);
    void Remove(ResearchProject project);

    // Members
    Task<ResearchProjectMember?> GetMembershipAsync(Guid projectId, Guid userId, CancellationToken cancellationToken);
    Task<ResearchProjectMember?> GetMemberByIdAsync(Guid memberId, CancellationToken cancellationToken);
    Task<List<ResearchProjectMember>> GetMembersAsync(Guid projectId, CancellationToken cancellationToken);
    Task AddMemberAsync(ResearchProjectMember member, CancellationToken cancellationToken);
    void RemoveMember(ResearchProjectMember member);

    // Tasks
    Task<ResearchTask?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken);
    Task<(List<ResearchTask> Items, int TotalCount)> GetTasksAsync(
        Guid projectId, ResearchTaskQueryParameters query, CancellationToken cancellationToken);
    Task AddTaskAsync(ResearchTask task, CancellationToken cancellationToken);
    void RemoveTask(ResearchTask task);

    // Milestones
    Task<ResearchMilestone?> GetMilestoneByIdAsync(Guid milestoneId, CancellationToken cancellationToken);
    Task<List<ResearchMilestone>> GetMilestonesAsync(Guid projectId, CancellationToken cancellationToken);
    Task<int> CountTasksForMilestoneAsync(Guid milestoneId, CancellationToken cancellationToken);
    Task AddMilestoneAsync(ResearchMilestone milestone, CancellationToken cancellationToken);
    void RemoveMilestone(ResearchMilestone milestone);

    // Notes
    Task<ResearchNote?> GetNoteByIdAsync(Guid noteId, CancellationToken cancellationToken);
    Task<List<ResearchNote>> GetNotesAsync(Guid projectId, CancellationToken cancellationToken);
    Task AddNoteAsync(ResearchNote note, CancellationToken cancellationToken);
    void RemoveNote(ResearchNote note);

    // Papers
    Task<ResearchPaper?> GetPaperByIdAsync(Guid paperId, CancellationToken cancellationToken);
    Task<List<ResearchPaper>> GetPapersAsync(Guid projectId, CancellationToken cancellationToken);
    Task AddPaperAsync(ResearchPaper paper, CancellationToken cancellationToken);
    void RemovePaper(ResearchPaper paper);

    // Publications
    Task<ResearchPublication?> GetPublicationByIdAsync(Guid publicationId, CancellationToken cancellationToken);
    Task<List<ResearchPublication>> GetPublicationsForProjectAsync(Guid projectId, CancellationToken cancellationToken);
    Task<(List<ResearchPublication> Items, int TotalCount)> GetPublicationRepositoryAsync(
        Guid userId, ResearchPublicationQueryParameters query, CancellationToken cancellationToken);
    Task AddPublicationAsync(ResearchPublication publication, CancellationToken cancellationToken);
    void RemovePublication(ResearchPublication publication);

    // Activity
    Task AddActivityAsync(ResearchActivity activity, CancellationToken cancellationToken);
    Task<List<ResearchActivity>> GetActivitiesAsync(Guid projectId, int take, CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}
