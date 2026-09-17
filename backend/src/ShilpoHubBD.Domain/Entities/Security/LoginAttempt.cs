namespace ShilpoHubBD.Domain.Entities.Security;

/// <summary>One login attempt, success or failure, for Threat Detection's failed-login analysis.</summary>
public class LoginAttempt
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public bool Succeeded { get; set; }
    public Guid? UserId { get; set; }

    public DateTime CreatedAt { get; set; }
}
