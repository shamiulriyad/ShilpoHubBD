using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.UnitTests.Common;

/// <summary>Builders for users and roles, in memory or ready to save to the test database.</summary>
public static class TestUsers
{
    public static User Create(string? email = null, string fullName = "Test User", bool isActive = true, string passwordHash = "hash")
    {
        var now = DateTime.UtcNow;
        return new User
        {
            Id = Guid.NewGuid(),
            Email = email ?? $"user-{Guid.NewGuid():N}@example.com",
            PasswordHash = passwordHash,
            FullName = fullName,
            IsActive = isActive,
            CreatedAt = now,
            UpdatedAt = now,
        };
    }

    public static Role Role(string name) => new() { Id = Guid.NewGuid(), Name = name };

    /// <summary>A role name no migration seeds, so tests can create it freely.</summary>
    public static string UniqueRoleName() => $"TestRole-{Guid.NewGuid():N}";

    public static User WithRoles(this User user, params Role[] roles)
    {
        foreach (var role in roles)
        {
            user.UserRoles.Add(new UserRole { UserId = user.Id, User = user, RoleId = role.Id, Role = role, AssignedAt = DateTime.UtcNow });
        }

        return user;
    }

    public static User WithRoles(this User user, params string[] roleNames)
        => user.WithRoles(roleNames.Select(Role).ToArray());
}
