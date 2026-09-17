namespace ShilpoHubBD.Application.DTOs.Security;

public class SystemHealthDto
{
    public bool DatabaseConnected { get; set; }
    public TimeSpan Uptime { get; set; }
    public int UserCount { get; set; }
    public int OrderCount { get; set; }
    public int ProductCount { get; set; }
    public long WorkingSetBytes { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}
