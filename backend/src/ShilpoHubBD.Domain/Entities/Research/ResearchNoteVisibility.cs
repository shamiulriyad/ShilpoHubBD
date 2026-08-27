namespace ShilpoHubBD.Domain.Entities.Research;

public enum ResearchNoteVisibility
{
    /// <summary>Visible only to the note author and project owners/admins.</summary>
    Private,

    /// <summary>Visible to every project member.</summary>
    Team,
}
