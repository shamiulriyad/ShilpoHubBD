namespace ShilpoHubBD.Application.DTOs.Security;

public class LoginAttemptDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public bool Succeeded { get; set; }
    public DateTime CreatedAt { get; set; }
}
