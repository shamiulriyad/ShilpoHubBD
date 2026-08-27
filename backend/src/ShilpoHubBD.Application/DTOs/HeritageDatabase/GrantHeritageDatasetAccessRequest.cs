namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class GrantHeritageDatasetAccessRequest
{
    public Guid UserId { get; set; }
    public string AccessRole { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
