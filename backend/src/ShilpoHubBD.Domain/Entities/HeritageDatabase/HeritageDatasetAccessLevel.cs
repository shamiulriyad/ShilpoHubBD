namespace ShilpoHubBD.Domain.Entities.HeritageDatabase;

public enum HeritageDatasetAccessLevel
{
    /// <summary>Any authenticated user may read.</summary>
    Public,

    /// <summary>Readable by research roles or holders of an access grant.</summary>
    Researcher,

    /// <summary>Readable only by the owner and explicit access grants.</summary>
    Restricted,
}
