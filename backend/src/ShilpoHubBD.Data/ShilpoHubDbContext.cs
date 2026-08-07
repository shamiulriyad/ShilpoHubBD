using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Marketplace;

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

	public DbSet<Category> Categories => Set<Category>();
	public DbSet<District> Districts => Set<District>();
	public DbSet<Product> Products => Set<Product>();
	public DbSet<ProductImage> ProductImages => Set<ProductImage>();
	public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
	public DbSet<CraftStory> CraftStories => Set<CraftStory>();
	public DbSet<CraftStoryChapter> CraftStoryChapters => Set<CraftStoryChapter>();
	public DbSet<ProducerStory> ProducerStories => Set<ProducerStory>();
	public DbSet<ProducerStoryChapter> ProducerStoryChapters => Set<ProducerStoryChapter>();
	public DbSet<WorkshopGalleryItem> WorkshopGalleryItems => Set<WorkshopGalleryItem>();

	public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
	public DbSet<CartItem> CartItems => Set<CartItem>();

	public DbSet<Order> Orders => Set<Order>();
	public DbSet<OrderItem> OrderItems => Set<OrderItem>();
	public DbSet<OrderStatusEvent> OrderStatusEvents => Set<OrderStatusEvent>();

	public DbSet<Payment> Payments => Set<Payment>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShilpoHubDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
