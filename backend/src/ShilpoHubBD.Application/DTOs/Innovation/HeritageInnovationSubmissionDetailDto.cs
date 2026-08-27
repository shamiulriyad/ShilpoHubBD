using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.Innovation;

public class HeritageInnovationSubmissionDetailDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Problem { get; set; } = string.Empty;
    public string Solution { get; set; } = string.Empty;
    public string? ResearchEvidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public Guid SubmitterUserId { get; set; }
    public string SubmitterName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public Guid? InnovationPrototypeId { get; set; }
    public Guid? PreservationStrategyId { get; set; }
    public Guid? HeritageDatasetId { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Guid? DecisionByUserId { get; set; }
    public string? DecisionByName { get; set; }
    public DateTime? DecisionAt { get; set; }
    public string? DecisionNote { get; set; }
    public bool CanReview { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SubmissionTeamMemberDto> TeamMembers { get; set; } = new();
    public List<SubmissionReviewDto> Reviews { get; set; } = new();
}
