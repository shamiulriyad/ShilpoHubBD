namespace ShilpoHubBD.Application.DTOs.Security;

public class CreateApiKeyRequest
{
    public string Name { get; set; } = string.Empty;
    public DateTime? ExpiresAt { get; set; }
}
