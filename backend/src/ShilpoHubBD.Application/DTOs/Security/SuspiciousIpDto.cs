namespace ShilpoHubBD.Application.DTOs.Security;

/// <summary>An IP address with several recent failed logins that is not yet blocked.</summary>
public class SuspiciousIpDto
{
    public string IpAddress { get; set; } = string.Empty;
    public int FailedAttempts { get; set; }
}
