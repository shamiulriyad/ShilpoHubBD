namespace ShilpoHubBD.Domain.Entities.Research;

/// <summary>
/// Project membership roles ordered by privilege. Higher numeric value = more privilege,
/// so guards can compare with <see cref="ResearchRoleExtensions.AtLeast"/>.
/// </summary>
public enum ResearchRole
{
    Viewer = 0,
    Contributor = 1,
    Researcher = 2,
    Admin = 3,
    Owner = 4,
}

public static class ResearchRoleExtensions
{
    public static bool AtLeast(this ResearchRole role, ResearchRole minimum) => role >= minimum;
}
