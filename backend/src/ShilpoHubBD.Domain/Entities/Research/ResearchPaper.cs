using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>A manuscript / paper draft managed inside a research project.</summary>
public class ResearchPaper
{
    public Guid Id { get; set; }

    public Guid ResearchProjectId { get; set; }
    public ResearchProject Project { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Abstract { get; set; }
    public string? Authors { get; set; }
    public string? Keywords { get; set; }

    public ResearchPaperStatus Status { get; set; } = ResearchPaperStatus.Draft;

    /// <summary>File reference only (URL / object key); no binary is stored by the backend.</summary>
    public string? ManuscriptUrl { get; set; }
    public string? TargetVenue { get; set; }

    public Guid CreatedByUserId { get; set; }
    public User CreatedBy { get; set; } = null!;

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<ResearchPublication> Publications { get; set; } = new List<ResearchPublication>();
}
