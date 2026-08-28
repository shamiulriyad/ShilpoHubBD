using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>A published output recorded in the project publication repository.</summary>
public class ResearchPublication
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public Guid? ResearchPaperId { get; set; }
    public ResearchPaper? Paper { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public string? Venue { get; set; }
    public ResearchPublicationType Type { get; set; } = ResearchPublicationType.JournalArticle;

    public string? Doi { get; set; }
    public string? Url { get; set; }
    public string? Abstract { get; set; }
    public string? Citation { get; set; }
    public DateTime? PublishedOn { get; set; }

    /// <summary>When true the entry is browsable through the public publication repository.</summary>
    public bool IsPublic { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
