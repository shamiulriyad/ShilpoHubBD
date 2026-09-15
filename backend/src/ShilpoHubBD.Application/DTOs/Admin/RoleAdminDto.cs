namespace ShilpoHubBD.Application.DTOs.Admin;

public class RoleAdminDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
}

public class RolePermissionsDto
{
    public Guid RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<string> PermissionCodes { get; set; } = new();
}

public class SyncRolePermissionsRequest
{
    public List<string> PermissionCodes { get; set; } = new();
}
