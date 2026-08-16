using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShilpoHubBD.Application.Interfaces.Services;
using ShilpoHubBD.Application.Options;
using ShilpoHubBD.Application.Services.Achievement;
using ShilpoHubBD.Application.Services.AIBusiness;
using ShilpoHubBD.Application.Services.AIBusinessPartner;
using ShilpoHubBD.Application.Services.AIShopping;
using ShilpoHubBD.Application.Services.Analytics;
using ShilpoHubBD.Application.Services.AITourism;
using ShilpoHubBD.Application.Services.ArVr;
using ShilpoHubBD.Application.Services.Auth;
using ShilpoHubBD.Application.Services.BusinessPartner;
using ShilpoHubBD.Application.Services.BusinessPartnerAnalytics;
using ShilpoHubBD.Application.Services.Commerce;
using ShilpoHubBD.Application.Services.Auction;
using ShilpoHubBD.Application.Services.Certificate;
using ShilpoHubBD.Application.Services.Community;
using ShilpoHubBD.Application.Services.Contracts;
using ShilpoHubBD.Application.Services.CSRSponsorship;
using ShilpoHubBD.Application.Services.CustomOrders;
using ShilpoHubBD.Application.Services.DesignCollaboration;
using ShilpoHubBD.Application.Services.HeritageDiscovery;
using ShilpoHubBD.Application.Services.HeritageIdentity;
using ShilpoHubBD.Application.Services.Impact;
using ShilpoHubBD.Application.Services.Inventory;
using ShilpoHubBD.Application.Services.Investment;
using ShilpoHubBD.Application.Services.Learning;
using ShilpoHubBD.Application.Services.LiveShopping;
using ShilpoHubBD.Application.Services.ManufacturingPartnership;
using ShilpoHubBD.Application.Services.Marketplace;
using ShilpoHubBD.Application.Services.Messaging;
using ShilpoHubBD.Application.Services.Passport;
using ShilpoHubBD.Application.Services.ProducerBusiness;
using ShilpoHubBD.Application.Services.ProducerComparison;
using ShilpoHubBD.Application.Services.ProductDevelopment;
using ShilpoHubBD.Application.Services.Procurement;
using ShilpoHubBD.Application.Services.Quotations;
using ShilpoHubBD.Application.Services.QRVerification;
using ShilpoHubBD.Application.Services.Recommendation;
using ShilpoHubBD.Application.Services.Reviews;
using ShilpoHubBD.Application.Services.Search;
using ShilpoHubBD.Application.Services.SupplierDiscovery;
using ShilpoHubBD.Application.Services.SupplierMatching;
using ShilpoHubBD.Application.Services.Sustainability;
using ShilpoHubBD.Application.Services.Traceability;
using ShilpoHubBD.Application.Services.TouristBooking;

namespace ShilpoHubBD.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.Configure<HeritageScoreOptions>(configuration.GetSection("HeritageScore"));
        services.Configure<SustainabilityScoreOptions>(configuration.GetSection("SustainabilityScore"));

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

        services.AddScoped<IHeritageIdentityService, HeritageIdentityService>();

        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<ICustomOrderService, CustomOrderService>();

        services.AddScoped<IProducerOrderService, ProducerOrderService>();
        services.AddScoped<IAIBusinessService, AIBusinessService>();

        services.AddScoped<IMentorService, MentorService>();
        services.AddScoped<ICourseService, CourseService>();
        services.AddScoped<IEnrollmentService, EnrollmentService>();
        services.AddScoped<ITrainingCertificateService, TrainingCertificateService>();

        services.AddScoped<ISustainabilityService, SustainabilityService>();

        services.AddScoped<IBusinessPartnerService, BusinessPartnerService>();
        services.AddScoped<ISupplierDiscoveryService, SupplierDiscoveryService>();
        services.AddScoped<ISupplierMatchingService, SupplierMatchingService>();
        services.AddScoped<IProducerComparisonService, ProducerComparisonService>();
        services.AddScoped<IQuotationService, QuotationService>();
        services.AddScoped<IProcurementService, ProcurementService>();
        services.AddScoped<IContractService, ContractService>();
        services.AddScoped<IPartnershipService, PartnershipService>();
        services.AddScoped<IBusinessPartnerAIService, BusinessPartnerAIService>();
        services.AddScoped<IDesignCollaborationService, DesignCollaborationService>();
        services.AddScoped<ICSRSponsorshipService, CSRSponsorshipService>();
        services.AddScoped<IProductDevelopmentService, ProductDevelopmentService>();
        services.AddScoped<IInvestmentService, InvestmentService>();
        services.AddScoped<IBusinessPartnerAnalyticsService, BusinessPartnerAnalyticsService>();

        services.AddScoped<IHeritagePlaceService, HeritagePlaceService>();
        services.AddScoped<IHeritageFestivalService, HeritageFestivalService>();
        services.AddScoped<ICulturalEventService, CulturalEventService>();
        services.AddScoped<ILocalCuisineService, LocalCuisineService>();

        services.AddScoped<ITouristServiceService, TouristServiceService>();
        services.AddScoped<IServiceAvailabilityService, ServiceAvailabilityService>();
        services.AddScoped<IBookingService, BookingService>();

        services.AddScoped<IAITourismService, AITourismService>();

        services.AddScoped<IMuseumItemService, MuseumItemService>();
        services.AddScoped<IVillageTourService, VillageTourService>();
        services.AddScoped<ICulturalStoryService, CulturalStoryService>();
        services.AddScoped<IArCraftScanService, ArCraftScanService>();

        return services;
    }
}
