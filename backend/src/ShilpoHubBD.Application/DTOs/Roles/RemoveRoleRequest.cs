namespace ShilpoHubBD.Application.DTOs.Roles;

public class RemoveRoleRequest
{
    public Guid UserId { get; set; }
    public string Role { get; set; } = string.Empty;
}
