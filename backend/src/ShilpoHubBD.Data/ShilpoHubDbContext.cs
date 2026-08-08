using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Achievement;
using ShilpoHubBD.Domain.Entities.Auction;
using ShilpoHubBD.Domain.Entities.Certificate;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Community;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.LiveShopping;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Messaging;
using ShilpoHubBD.Domain.Entities.Passport;
using ShilpoHubBD.Domain.Entities.QRVerification;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Domain.Entities.Traceability;

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

	public DbSet<Review> Reviews => Set<Review>();
	public DbSet<ReviewImage> ReviewImages => Set<ReviewImage>();

	public DbSet<CommunityQuestion> CommunityQuestions => Set<CommunityQuestion>();
	public DbSet<CommunityAnswer> CommunityAnswers => Set<CommunityAnswer>();
	public DbSet<DiscussionThread> DiscussionThreads => Set<DiscussionThread>();
	public DbSet<DiscussionReply> DiscussionReplies => Set<DiscussionReply>();
	public DbSet<ProducerFollow> ProducerFollows => Set<ProducerFollow>();
	public DbSet<Village> Villages => Set<Village>();
	public DbSet<VillageFavorite> VillageFavorites => Set<VillageFavorite>();

	public DbSet<Conversation> Conversations => Set<Conversation>();
	public DbSet<ConversationParticipant> ConversationParticipants => Set<ConversationParticipant>();
	public DbSet<Message> Messages => Set<Message>();

	public DbSet<LiveEvent> LiveEvents => Set<LiveEvent>();
	public DbSet<LiveEventComment> LiveEventComments => Set<LiveEventComment>();
	public DbSet<LiveEventReaction> LiveEventReactions => Set<LiveEventReaction>();
	public DbSet<LiveEventPurchase> LiveEventPurchases => Set<LiveEventPurchase>();

	public DbSet<Auction> Auctions => Set<Auction>();
	public DbSet<AuctionBid> AuctionBids => Set<AuctionBid>();

	public DbSet<QRCode> QRCodes => Set<QRCode>();
	public DbSet<QRVerificationRecord> QRVerificationRecords => Set<QRVerificationRecord>();

	public DbSet<Certificate> Certificates => Set<Certificate>();

	public DbSet<ProductTraceability> ProductTraceabilities => Set<ProductTraceability>();
	public DbSet<MaterialSource> MaterialSources => Set<MaterialSource>();
	public DbSet<TimelineEvent> TimelineEvents => Set<TimelineEvent>();

	public DbSet<Badge> Badges => Set<Badge>();
	public DbSet<UserBadge> UserBadges => Set<UserBadge>();

	public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();
	public DbSet<Achievement> Achievements => Set<Achievement>();
	public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShilpoHubDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
