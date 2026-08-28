using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>
/// A collaborative heritage research project. Acts as the aggregate root for members,
/// tasks, milestones, notes, papers, publications and activity history.
/// </summary>
public class ResearchProject
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Discipline { get; set; }
    public string? Institution { get; set; }

    public ResearchProjectStatus Status { get; set; } = ResearchProjectStatus.Planning;
    public ResearchProjectVisibility Visibility { get; set; } = ResearchProjectVisibility.Private;

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ResearchProjectMember> Members { get; set; } = new List<ResearchProjectMember>();
    public ICollection<ResearchTask> Tasks { get; set; } = new List<ResearchTask>();
    public ICollection<ResearchMilestone> Milestones { get; set; } = new List<ResearchMilestone>();
    public ICollection<ResearchNote> Notes { get; set; } = new List<ResearchNote>();
    public ICollection<ResearchPaper> Papers { get; set; } = new List<ResearchPaper>();
    public ICollection<ResearchPublication> Publications { get; set; } = new List<ResearchPublication>();
    public ICollection<ResearchActivity> Activities { get; set; } = new List<ResearchActivity>();
}
