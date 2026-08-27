using System.Collections.Generic;

namespace ShilpoHubBD.Application.DTOs.FieldResearch;

public class SurveyDetailDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Objective { get; set; }
    public string? TargetRegion { get; set; }
    public string Language { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool AllowAnonymousResponses { get; set; }
    public Guid OwnerUserId { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public Guid? ResearchProjectId { get; set; }
    public string MyRole { get; set; } = string.Empty;
    public bool CanManage { get; set; }
    public DateTime? OpensAt { get; set; }
    public DateTime? ClosesAt { get; set; }
    public int ResponseCount { get; set; }
    public int EvidenceCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<SurveyQuestionDto> Questions { get; set; } = new();
    public List<SurveyFieldAssignmentDto> FieldAssignments { get; set; } = new();
}
