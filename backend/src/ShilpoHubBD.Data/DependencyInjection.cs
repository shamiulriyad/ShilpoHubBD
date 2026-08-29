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
		services.AddScoped<IHeritageCheckInRepository, HeritageCheckInRepository>();
		services.AddScoped<ITravelJournalRepository, TravelJournalRepository>();
		services.AddScoped<IAchievementRepository, AchievementRepository>();
		services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
		services.AddScoped<ITouristAnalyticsRepository, TouristAnalyticsRepository>();
		services.AddScoped<IImpactRepository, ImpactRepository>();
		services.AddScoped<IHeritageIdentityRepository, HeritageIdentityRepository>();
		services.AddScoped<IInventoryRepository, InventoryRepository>();
		services.AddScoped<ICustomOrderRepository, CustomOrderRepository>();
		services.AddScoped<IProducerOrderRepository, ProducerOrderRepository>();

		services.AddScoped<IMentorRepository, MentorRepository>();
		services.AddScoped<ICourseRepository, CourseRepository>();
		services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
		services.AddScoped<ITrainingCertificateRepository, TrainingCertificateRepository>();
		services.AddScoped<IHeritageSkillRepository, HeritageSkillRepository>();
		services.AddScoped<IAcademyMemberProfileRepository, AcademyMemberProfileRepository>();
		services.AddScoped<ICourseCategoryRepository, CourseCategoryRepository>();
		services.AddScoped<ILiveClassRepository, LiveClassRepository>();
		services.AddScoped<IAssignmentRepository, AssignmentRepository>();
		services.AddScoped<IQuizRepository, QuizRepository>();
		services.AddScoped<IExamRepository, ExamRepository>();
		services.AddScoped<ISkillAssessmentRepository, SkillAssessmentRepository>();
		services.AddScoped<ILearningRoadmapRepository, LearningRoadmapRepository>();
		services.AddScoped<IMentorMatchingRepository, MentorMatchingRepository>();
		services.AddScoped<IMentorshipRequestRepository, MentorshipRequestRepository>();
		services.AddScoped<IApprenticeshipProgramRepository, ApprenticeshipProgramRepository>();
		services.AddScoped<IProgramApplicationRepository, ProgramApplicationRepository>();
		services.AddScoped<IApprenticeEnrollmentRepository, ApprenticeEnrollmentRepository>();
		services.AddScoped<IPortfolioRepository, PortfolioRepository>();
		services.AddScoped<IMentorFeedbackRepository, MentorFeedbackRepository>();
		services.AddScoped<IJobListingRepository, JobListingRepository>();
		services.AddScoped<IJobApplicationRepository, JobApplicationRepository>();

		services.AddScoped<ISustainabilityRepository, SustainabilityRepository>();

		services.AddScoped<IBusinessPartnerRepository, BusinessPartnerRepository>();
		services.AddScoped<ISupplierDiscoveryRepository, SupplierDiscoveryRepository>();
		services.AddScoped<ISupplierMatchingRepository, SupplierMatchingRepository>();
		services.AddScoped<IProducerComparisonRepository, ProducerComparisonRepository>();
		services.AddScoped<IQuotationRepository, QuotationRepository>();
		services.AddScoped<IProcurementRepository, ProcurementRepository>();
		services.AddScoped<IContractRepository, ContractRepository>();
		services.AddScoped<IPartnershipRepository, PartnershipRepository>();
		services.AddScoped<IAIIntelligenceRepository, AIIntelligenceRepository>();
		services.AddScoped<IDesignCollaborationRepository, DesignCollaborationRepository>();
		services.AddScoped<ICSRSponsorshipRepository, CSRSponsorshipRepository>();
		services.AddScoped<IProductDevelopmentRepository, ProductDevelopmentRepository>();
		services.AddScoped<IInvestmentRepository, InvestmentRepository>();
		services.AddScoped<IBusinessPartnerAnalyticsRepository, BusinessPartnerAnalyticsRepository>();

		services.AddScoped<IHeritagePlaceRepository, HeritagePlaceRepository>();
		services.AddScoped<IHeritageFestivalRepository, HeritageFestivalRepository>();
		services.AddScoped<ICulturalEventRepository, CulturalEventRepository>();
		services.AddScoped<ILocalCuisineRepository, LocalCuisineRepository>();
		services.AddScoped<IHeritageRouteRepository, HeritageRouteRepository>();

		services.AddScoped<ITouristServiceRepository, TouristServiceRepository>();
		services.AddScoped<IServiceAvailabilitySlotRepository, ServiceAvailabilitySlotRepository>();
		services.AddScoped<IBookingRepository, BookingRepository>();

		services.AddScoped<IResearchProjectRepository, ResearchProjectRepository>();
		services.AddScoped<IResearchAIAnalysisRepository, ResearchAIAnalysisRepository>();
		services.AddScoped<ISurveyRepository, SurveyRepository>();
		services.AddScoped<IKnowledgeGraphRepository, KnowledgeGraphRepository>();

		services.AddScoped<IInnovationLinkResolver, InnovationLinkResolver>();
		services.AddScoped<IInnovationExperimentRepository, InnovationExperimentRepository>();
		services.AddScoped<IPreservationStrategyRepository, PreservationStrategyRepository>();
		services.AddScoped<IInnovationPrototypeRepository, InnovationPrototypeRepository>();
		services.AddScoped<IHeritageInnovationSubmissionRepository, HeritageInnovationSubmissionRepository>();

		services.AddScoped<IHeritageDatasetRepository, HeritageDatasetRepository>();
		services.AddScoped<IHeritageDataRepository, HeritageDataRepository>();
		services.AddScoped<IHeritageRiskRepository, HeritageRiskRepository>();

		services.AddScoped<INationalDashboardRepository, NationalDashboardRepository>();
		services.AddScoped<IHeritageIntelligenceRepository, HeritageIntelligenceRepository>();
		services.AddScoped<IPolicySimulationRepository, PolicySimulationRepository>();
		services.AddScoped<IMonitoringRepository, MonitoringRepository>();
		services.AddScoped<IComplaintRepository, ComplaintRepository>();
		services.AddScoped<IComplianceRepository, ComplianceRepository>();
		services.AddScoped<IFundingRepository, FundingRepository>();
		services.AddScoped<IGovAnalyticsRepository, GovAnalyticsRepository>();

		services.AddScoped<ILogisticsPartnerRepository, LogisticsPartnerRepository>();
		services.AddScoped<IPickupRequestRepository, PickupRequestRepository>();
		services.AddScoped<IRouteOptimizationRepository, RouteOptimizationRepository>();
		services.AddScoped<IDeliveryTrackingRepository, DeliveryTrackingRepository>();
		services.AddScoped<IWarehouseRepository, WarehouseRepository>();
		services.AddScoped<IWarehouseStockRepository, WarehouseStockRepository>();

		services.AddScoped<IMuseumItemRepository, MuseumItemRepository>();
		services.AddScoped<IVillageTourStopRepository, VillageTourStopRepository>();
		services.AddScoped<ICulturalStoryRepository, CulturalStoryRepository>();

		return services;
	}
}
