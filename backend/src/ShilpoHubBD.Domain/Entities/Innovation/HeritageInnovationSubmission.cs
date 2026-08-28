using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Domain.Entities.Innovation;

/// <summary>A submitted heritage innovation for review and approval/rejection.</summary>
public class HeritageInnovationSubmission
{
    public Guid Id { get; set; }

    public Guid SubmitterUserId { get; set; }
    public User Submitter { get; set; } = null!;

    public Guid? ResearchProjectId { get; set; }
    public ResearchProject? ResearchProject { get; set; }

    public Guid? InnovationPrototypeId { get; set; }
    public InnovationPrototype? Prototype { get; set; }

    public Guid? PreservationStrategyId { get; set; }
    public PreservationStrategy? PreservationStrategy { get; set; }

    public Guid? HeritageDatasetId { get; set; }
    public HeritageDataset? HeritageDataset { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string? ResearchEvidence { get; set; }

    public InnovationSubmissionStatus Status { get; set; } = InnovationSubmissionStatus.Draft;

    public DateTime? SubmittedAt { get; set; }
    public Guid? DecisionByUserId { get; set; }
    public User? DecisionBy { get; set; }
    public DateTime? DecisionAt { get; set; }
    public string? DecisionNote { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<SubmissionTeamMember> TeamMembers { get; set; } = new List<SubmissionTeamMember>();
    public ICollection<SubmissionReview> Reviews { get; set; } = new List<SubmissionReview>();
    public ICollection<SubmissionEvent> History { get; set; } = new List<SubmissionEvent>();
}
