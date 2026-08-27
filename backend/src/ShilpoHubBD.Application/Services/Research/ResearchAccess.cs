using ShilpoHubBD.Application.Exceptions;
using ShilpoHubBD.Domain.Entities.Research;

namespace ShilpoHubBD.Application.Services.Research;

/// <summary>
/// Central authorization helpers for the Research Workspace. Membership is the primary gate:
/// a caller who is not a member of a non-public project is told the project does not exist.
/// </summary>
internal static class ResearchAccess
{
    public static void RequireReadAccess(ResearchProject project, ResearchProjectMember? membership)
    {
        if (membership is null && project.Visibility != ResearchProjectVisibility.Public)
        {
            throw new NotFoundException("Research project not found.");
        }
    }

    public static ResearchProjectMember RequireMember(ResearchProjectMember? membership)
        => membership ?? throw new NotFoundException("Research project not found.");

    public static ResearchProjectMember RequireRole(ResearchProjectMember? membership, ResearchRole minimum)
    {
        var member = RequireMember(membership);
        if (!member.Role.AtLeast(minimum))
        {
            throw new UnauthorizedAccessException(
                $"This action requires the {minimum} role or higher on this research project.");
        }

        return member;
    }
}
