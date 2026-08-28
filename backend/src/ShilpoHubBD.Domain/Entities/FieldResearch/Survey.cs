using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Domain.Entities.FieldResearch;

/// <summary>
/// A digital heritage-research survey. Owned and managed by a researcher; optionally attached to a
/// <see cref="ResearchProject"/>. Field researchers are assigned to collect responses in the field.
/// </summary>
public class Survey
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }
    public User Owner { get; set; } = null!;

    public Guid? ResearchProjectId { get; set; }
    public ResearchProject? ResearchProject { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string? TargetRegion { get; set; }
    public string Language { get; set; } = "bn";

    public SurveyStatus Status { get; set; } = SurveyStatus.Draft;
    public bool AllowAnonymousResponses { get; set; }

    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }

    public int ResponseCount { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
    public ICollection<SurveyFieldAssignment> FieldAssignments { get; set; } = new List<SurveyFieldAssignment>();
    public ICollection<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
    public ICollection<FieldEvidence> Evidence { get; set; } = new List<FieldEvidence>();
    public ICollection<DataCollectionEvent> CollectionEvents { get; set; } = new List<DataCollectionEvent>();
}
