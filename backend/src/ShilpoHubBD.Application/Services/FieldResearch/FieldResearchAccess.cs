using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Domain.Entities.FieldResearch;

namespace ShilpoHubBD.Application.Services.FieldResearch;

/// <summary>
/// Authorization helpers for the Survey &amp; Field Data Collection module. The survey owner manages
/// everything; assigned active field researchers collect data; Supervisor / Reviewer assignments may
/// also review responses.
/// </summary>
internal static class FieldResearchAccess
{
    public static bool IsOwner(Survey survey, Guid userId) => survey.OwnerUserId == userId;

    public static SurveyFieldAssignment? ActiveAssignment(Survey survey, Guid userId)
        => survey.FieldAssignments.FirstOrDefault(a => a.FieldResearcherUserId == userId && a.IsActive);

    public static bool AnyAssignment(Survey survey, Guid userId)
        => survey.FieldAssignments.Any(a => a.FieldResearcherUserId == userId);

    public static void RequireOwner(Survey survey, Guid userId)
    {
        if (!IsOwner(survey, userId))
        {
            throw new UnauthorizedAccessException("Only the survey owner can perform this action.");
        }
    }

    /// <summary>Owner or an active field-researcher assignment.</summary>
    public static void RequireContributor(Survey survey, Guid userId)
    {
        if (!IsOwner(survey, userId) && ActiveAssignment(survey, userId) is null)
        {
            throw new UnauthorizedAccessException(
                "You must be the survey owner or an active assigned field researcher.");
        }
    }

    public static void RequireReviewer(Survey survey, Guid userId)
    {
        if (IsOwner(survey, userId))
        {
            return;
        }

        var assignment = ActiveAssignment(survey, userId);
        if (assignment is null
            || (assignment.Role != FieldAssignmentRole.Supervisor && assignment.Role != FieldAssignmentRole.Reviewer))
        {
            throw new UnauthorizedAccessException(
                "Only the survey owner or a Supervisor / Reviewer field researcher can review responses.");
        }
    }
}
