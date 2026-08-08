using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Repositories;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Data.Repositories;
using ShilpoHubBD.Data.Search;

namespace ShilpoHubBD.Data;

public static class DependencyInjection
{
	public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
	{
		var connectionString = configuration.GetConnectionString("DefaultConnection");
		if (string.IsNullOrWhiteSpace(connectionString))
		{
			throw new InvalidOperationException(
				"Connection string 'DefaultConnection' is not configured. Set ConnectionStrings__DefaultConnection " +
				"to your Supabase Postgres connection string.");
		}

		services.AddDbContext<ShilpoHubDbContext>(options => options.UseNpgsql(connectionString));

		services.AddScoped<IUserRepository, UserRepository>();
		services.AddScoped<IRoleRepository, RoleRepository>();
		services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
		services.AddScoped<IPasswordResetTokenRepository, PasswordResetTokenRepository>();

		services.AddScoped<IProductRepository, ProductRepository>();
		services.AddScoped<ICategoryRepository, CategoryRepository>();
		services.AddScoped<IDistrictRepository, DistrictRepository>();
		services.AddScoped<ICraftStoryRepository, CraftStoryRepository>();
		services.AddScoped<IProducerStoryRepository, ProducerStoryRepository>();
		services.AddScoped<IWorkshopGalleryRepository, WorkshopGalleryRepository>();
		services.AddScoped<IWishlistRepository, WishlistRepository>();
		services.AddScoped<ICartRepository, CartRepository>();
		services.AddScoped<IOrderRepository, OrderRepository>();
		services.AddScoped<IPaymentRepository, PaymentRepository>();
		services.AddScoped<IReviewRepository, ReviewRepository>();
		services.AddScoped<IQuestionRepository, QuestionRepository>();
		services.AddScoped<IDiscussionRepository, DiscussionRepository>();
		services.AddScoped<IProducerFollowRepository, ProducerFollowRepository>();
		services.AddScoped<IVillageRepository, VillageRepository>();
		services.AddScoped<IMessagingRepository, MessagingRepository>();
		services.AddScoped<ILiveShoppingRepository, LiveShoppingRepository>();
		services.AddScoped<IAuctionRepository, AuctionRepository>();
		services.AddScoped<IQRVerificationRepository, QRVerificationRepository>();
		services.AddScoped<ICertificateRepository, CertificateRepository>();
		services.AddScoped<ITraceabilityRepository, TraceabilityRepository>();
		services.AddScoped<ISearchProvider, PostgresProductSearchProvider>();
		services.AddScoped<IPassportRepository, PassportRepository>();
		services.AddScoped<IAchievementRepository, AchievementRepository>();
		services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
		services.AddScoped<IImpactRepository, ImpactRepository>();

		return services;
	}
}
