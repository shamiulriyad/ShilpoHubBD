using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.FieldResearch;

/// <summary>Assigns a field researcher to a survey with a collection role and (optionally) an area.</summary>
public class SurveyFieldAssignment
{
    public Guid Id { get; set; }

    public Guid SurveyId { get; set; }
    public Survey Survey { get; set; } = null!;

    public Guid FieldResearcherUserId { get; set; }
    public User FieldResearcher { get; set; } = null!;

    public FieldAssignmentRole Role { get; set; } = FieldAssignmentRole.Collector;
    public string? AreaNote { get; set; }
    public bool IsActive { get; set; } = true;

    public Guid AssignedByUserId { get; set; }
    public User AssignedBy { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
}
