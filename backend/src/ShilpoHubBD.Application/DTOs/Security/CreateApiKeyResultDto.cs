namespace ShilpoHubBD.Application.DTOs.Security;

/// <summary>Returned only once, at creation — the raw key is never retrievable again.</summary>
public class CreateApiKeyResultDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string KeyPrefix { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
