namespace ShilpoHubBD.Application.DTOs.Security;

public class BlockIpRequest
{
    public string IpAddress { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
