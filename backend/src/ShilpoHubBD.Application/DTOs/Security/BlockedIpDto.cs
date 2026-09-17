namespace ShilpoHubBD.Application.DTOs.Security;

public class BlockedIpDto
{
    public Guid Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public string BlockedByName { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
