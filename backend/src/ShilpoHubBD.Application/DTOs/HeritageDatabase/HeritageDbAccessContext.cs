namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

/// <summary>Caller identity plus the role facts the Heritage Database services need for access checks.</summary>
public class HeritageDbAccessContext
{
    public Guid UserId { get; set; }

    /// <summary>True when the caller holds a research role (HeritageInnovationHub / GovernmentNGO / SuperAdmin).</summary>
    public bool IsResearcher { get; set; }

    public bool IsSuperAdmin { get; set; }
}
