namespace ShilpoHubBD.Domain.Entities.Identity;

public class UserRole
{
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime AssignedAt { get; set; }
    public Guid? AssignedByUserId { get; set; }
}
