using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Domain.Entities.Admin;

/// <summary>Grants a <see cref="Permission"/> to a <see cref="Role"/>.</summary>
public class RolePermission
{
    public Guid Id { get; set; }

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public Guid PermissionId { get; set; }
    public Permission Permission { get; set; } = null!;

    public DateTime GrantedAt { get; set; }
    public Guid? GrantedByUserId { get; set; }
    public User? GrantedBy { get; set; }
}
