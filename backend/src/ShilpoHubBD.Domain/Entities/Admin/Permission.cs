namespace ShilpoHubBD.Domain.Entities.Admin;

/// <summary>A granular capability that can be granted to a role (e.g. "users.manage").</summary>
public class Permission
{
    public Guid Id { get; set; }

    /// <summary>Stable machine key, e.g. "users.manage". Unique.</summary>
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    /// <summary>Grouping shown in the admin UI, e.g. "Users", "Heritage", "Marketplace".</summary>
    public string Module { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
