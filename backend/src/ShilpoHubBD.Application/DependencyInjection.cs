using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Services.Achievement;
using ShilpoHubBD.Application.Services.AIShopping;
using ShilpoHubBD.Application.Services.Analytics;
using ShilpoHubBD.Application.Services.Auth;
using ShilpoHubBD.Application.Services.Commerce;
using ShilpoHubBD.Application.Services.Auction;
using ShilpoHubBD.Application.Services.Certificate;
using ShilpoHubBD.Application.Services.Community;
using ShilpoHubBD.Application.Services.Impact;
using ShilpoHubBD.Application.Services.LiveShopping;
using ShilpoHubBD.Application.Services.Marketplace;
using ShilpoHubBD.Application.Services.Messaging;
using ShilpoHubBD.Application.Services.Passport;
using ShilpoHubBD.Application.Services.QRVerification;
using ShilpoHubBD.Application.Services.Recommendation;
using ShilpoHubBD.Application.Services.Reviews;
using ShilpoHubBD.Application.Services.Search;
using ShilpoHubBD.Application.Services.Traceability;

namespace ShilpoHubBD.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IRoleService, RoleService>();

        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IDistrictService, DistrictService>();
        services.AddScoped<ICraftStoryService, CraftStoryService>();
        services.AddScoped<IProducerStoryService, ProducerStoryService>();
        services.AddScoped<IWorkshopGalleryService, WorkshopGalleryService>();

        services.AddScoped<IWishlistService, WishlistService>();
        services.AddScoped<ICartService, CartService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IPaymentService, PaymentService>();
        services.AddScoped<IReviewService, ReviewService>();

        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IDiscussionService, DiscussionService>();
        services.AddScoped<IProducerFollowService, ProducerFollowService>();
        services.AddScoped<IVillageService, VillageService>();

        services.AddScoped<IMessagingService, MessagingService>();

        services.AddScoped<ILiveShoppingService, LiveShoppingService>();

        services.AddScoped<IAuctionService, AuctionService>();

        services.AddScoped<IQRVerificationService, QRVerificationService>();

        services.AddScoped<ICertificateService, CertificateService>();

        services.AddScoped<ITraceabilityService, TraceabilityService>();

        services.AddScoped<IRecommendationService, RecommendationService>();

        services.AddScoped<ISearchService, SearchService>();

        services.AddScoped<IGiftRecommendationService, GiftRecommendationService>();
        services.AddScoped<IFashionMatchingService, FashionMatchingService>();
        services.AddScoped<IInteriorPreviewService, InteriorPreviewService>();
        services.AddScoped<ITranslationService, TranslationService>();

        services.AddScoped<IPassportService, PassportService>();

        services.AddScoped<IAchievementService, AchievementService>();

        services.AddScoped<IAnalyticsService, AnalyticsService>();

        services.AddScoped<IImpactService, ImpactService>();

        return services;
    }
}
