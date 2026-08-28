namespace ShilpoHubBD.Application.DTOs.HeritageDatabase;

public class HeritageDatasetAccessGrantDto
{
    public Guid Id { get; set; }
    public Guid HeritageDatasetId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string AccessRole { get; set; } = string.Empty;
    public Guid GrantedByUserId { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}
