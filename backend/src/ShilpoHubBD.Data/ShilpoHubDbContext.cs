using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Identity;

namespace ShilpoHubBD.Data;

public class ShilpoHubDbContext : DbContext
{
	public ShilpoHubDbContext(DbContextOptions<ShilpoHubDbContext> options) : base(options)
	{
	}

	public DbSet<User> Users => Set<User>();
	public DbSet<Role> Roles => Set<Role>();
	public DbSet<UserRole> UserRoles => Set<UserRole>();
	public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
	public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShilpoHubDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
