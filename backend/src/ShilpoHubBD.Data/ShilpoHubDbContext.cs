using Microsoft.EntityFrameworkCore;
using ShilpoHubBD.Domain.Entities.Achievement;
using ShilpoHubBD.Domain.Entities.Apprenticeship;
using ShilpoHubBD.Domain.Entities.Assessment;
using ShilpoHubBD.Domain.Entities.ArVr;
using ShilpoHubBD.Domain.Entities.Auction;
using ShilpoHubBD.Domain.Entities.BusinessPartner;
using ShilpoHubBD.Domain.Entities.Certificate;
using ShilpoHubBD.Domain.Entities.Commerce;
using ShilpoHubBD.Domain.Entities.Community;
using ShilpoHubBD.Domain.Entities.Contracts;
using ShilpoHubBD.Domain.Entities.CSRSponsorship;
using ShilpoHubBD.Domain.Entities.CustomOrders;
using ShilpoHubBD.Domain.Entities.DesignCollaboration;
using ShilpoHubBD.Domain.Entities.Employment;
using ShilpoHubBD.Domain.Entities.FieldResearch;
using ShilpoHubBD.Domain.Entities.Governance;
using ShilpoHubBD.Domain.Entities.HeritageDatabase;
using ShilpoHubBD.Domain.Entities.HeritageDiscovery;
using ShilpoHubBD.Domain.Entities.HeritageIdentity;
using ShilpoHubBD.Domain.Entities.Identity;
using ShilpoHubBD.Domain.Entities.Innovation;
using ShilpoHubBD.Domain.Entities.Inventory;
using ShilpoHubBD.Domain.Entities.Investment;
using ShilpoHubBD.Domain.Entities.KnowledgeGraph;
using ShilpoHubBD.Domain.Entities.Learning;
using ShilpoHubBD.Domain.Entities.LiveClass;
using ShilpoHubBD.Domain.Entities.LiveShopping;
using ShilpoHubBD.Domain.Entities.ManufacturingPartnership;
using ShilpoHubBD.Domain.Entities.Marketplace;
using ShilpoHubBD.Domain.Entities.Mentorship;
using ShilpoHubBD.Domain.Entities.Messaging;
using ShilpoHubBD.Domain.Entities.Passport;
using ShilpoHubBD.Domain.Entities.Portfolio;
using ShilpoHubBD.Domain.Entities.Procurement;
using ShilpoHubBD.Domain.Entities.ProductDevelopment;
using ShilpoHubBD.Domain.Entities.QRVerification;
using ShilpoHubBD.Domain.Entities.Research;
using ShilpoHubBD.Domain.Entities.Quotations;
using ShilpoHubBD.Domain.Entities.Reviews;
using ShilpoHubBD.Domain.Entities.Roadmap;
using ShilpoHubBD.Domain.Entities.SkillAssessment;
using ShilpoHubBD.Domain.Entities.Sustainability;
using ShilpoHubBD.Domain.Entities.Traceability;
using ShilpoHubBD.Domain.Entities.TouristBooking;

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
	public DbSet<ProductVideo> ProductVideos => Set<ProductVideo>();
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
	public DbSet<HeritageCheckIn> HeritageCheckIns => Set<HeritageCheckIn>();
	public DbSet<TravelJournalEntry> TravelJournalEntries => Set<TravelJournalEntry>();

	public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();
	public DbSet<Achievement> Achievements => Set<Achievement>();
	public DbSet<UserAchievement> UserAchievements => Set<UserAchievement>();

	public DbSet<ProducerHeritageIdentity> ProducerHeritageIdentities => Set<ProducerHeritageIdentity>();
	public DbSet<FamilyHeritageMember> FamilyHeritageMembers => Set<FamilyHeritageMember>();
	public DbSet<SkillTimelineEntry> SkillTimelineEntries => Set<SkillTimelineEntry>();
	public DbSet<HeritageAward> HeritageAwards => Set<HeritageAward>();
	public DbSet<HeritageCertification> HeritageCertifications => Set<HeritageCertification>();
	public DbSet<StoryArchiveEntry> StoryArchiveEntries => Set<StoryArchiveEntry>();
	public DbSet<ScoreHistoryEntry> HeritageScoreHistory => Set<ScoreHistoryEntry>();

	public DbSet<InventoryTransaction> InventoryTransactions => Set<InventoryTransaction>();
	public DbSet<CustomOrderRequest> CustomOrderRequests => Set<CustomOrderRequest>();

	public DbSet<MentorProfile> MentorProfiles => Set<MentorProfile>();
	public DbSet<MentorSkill> MentorSkills => Set<MentorSkill>();
	public DbSet<MentorshipRequest> MentorshipRequests => Set<MentorshipRequest>();
	public DbSet<HeritageSkill> HeritageSkills => Set<HeritageSkill>();
	public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
	public DbSet<CourseModule> CourseModules => Set<CourseModule>();
	public DbSet<CourseMaterial> CourseMaterials => Set<CourseMaterial>();
	public DbSet<AcademyMemberProfile> AcademyMemberProfiles => Set<AcademyMemberProfile>();
	public DbSet<AcademyMemberSkill> AcademyMemberSkills => Set<AcademyMemberSkill>();
	public DbSet<Course> Courses => Set<Course>();
	public DbSet<CourseLesson> CourseLessons => Set<CourseLesson>();
	public DbSet<CourseEnrollment> CourseEnrollments => Set<CourseEnrollment>();
	public DbSet<LessonProgress> LessonProgress => Set<LessonProgress>();
	public DbSet<TrainingCertificate> TrainingCertificates => Set<TrainingCertificate>();

	public DbSet<ApprenticeshipProgram> ApprenticeshipPrograms => Set<ApprenticeshipProgram>();
	public DbSet<TrainingMilestone> TrainingMilestones => Set<TrainingMilestone>();
	public DbSet<ProgramApplication> ProgramApplications => Set<ProgramApplication>();
	public DbSet<ApprenticeEnrollment> ApprenticeEnrollments => Set<ApprenticeEnrollment>();
	public DbSet<ApprenticeMilestoneProgress> ApprenticeMilestoneProgress => Set<ApprenticeMilestoneProgress>();

	public DbSet<Portfolio> Portfolios => Set<Portfolio>();
	public DbSet<PortfolioProject> PortfolioProjects => Set<PortfolioProject>();
	public DbSet<MentorFeedback> MentorFeedbacks => Set<MentorFeedback>();

	public DbSet<JobListing> JobListings => Set<JobListing>();
	public DbSet<JobSkillRequirement> JobSkillRequirements => Set<JobSkillRequirement>();
	public DbSet<JobApplication> JobApplications => Set<JobApplication>();

	public DbSet<SustainabilityProfile> SustainabilityProfiles => Set<SustainabilityProfile>();
	public DbSet<SustainableMaterialRecord> SustainableMaterialRecords => Set<SustainableMaterialRecord>();
	public DbSet<SustainableMaterialCertification> SustainableMaterialCertifications => Set<SustainableMaterialCertification>();

	public DbSet<BusinessPartnerProfile> BusinessPartnerProfiles => Set<BusinessPartnerProfile>();
	public DbSet<BusinessDocument> BusinessDocuments => Set<BusinessDocument>();
	public DbSet<BusinessPartnerPreferredCategory> BusinessPartnerPreferredCategories => Set<BusinessPartnerPreferredCategory>();

	public DbSet<QuotationRequest> QuotationRequests => Set<QuotationRequest>();
	public DbSet<QuotationRequestItem> QuotationRequestItems => Set<QuotationRequestItem>();
	public DbSet<QuotationRequestProducer> QuotationRequestProducers => Set<QuotationRequestProducer>();
	public DbSet<QuotationResponse> QuotationResponses => Set<QuotationResponse>();
	public DbSet<QuotationResponseItem> QuotationResponseItems => Set<QuotationResponseItem>();
	public DbSet<QuotationStatusEvent> QuotationStatusEvents => Set<QuotationStatusEvent>();

	public DbSet<ProcurementRequest> ProcurementRequests => Set<ProcurementRequest>();
	public DbSet<ProcurementItem> ProcurementItems => Set<ProcurementItem>();
	public DbSet<ProcurementStatusEvent> ProcurementStatusEvents => Set<ProcurementStatusEvent>();

	public DbSet<Contract> Contracts => Set<Contract>();
	public DbSet<ContractItem> ContractItems => Set<ContractItem>();
	public DbSet<ContractDeliverySchedule> ContractDeliverySchedules => Set<ContractDeliverySchedule>();
	public DbSet<ContractDocument> ContractDocuments => Set<ContractDocument>();
	public DbSet<ContractStatusEvent> ContractStatusEvents => Set<ContractStatusEvent>();

	public DbSet<ManufacturingPartnership> ManufacturingPartnerships => Set<ManufacturingPartnership>();
	public DbSet<ManufacturingMilestone> ManufacturingMilestones => Set<ManufacturingMilestone>();
	public DbSet<PartnershipStatusEvent> PartnershipStatusEvents => Set<PartnershipStatusEvent>();

	public DbSet<DesignCollaborationProject> DesignCollaborationProjects => Set<DesignCollaborationProject>();
	public DbSet<DesignFile> DesignFiles => Set<DesignFile>();
	public DbSet<DesignComment> DesignComments => Set<DesignComment>();
	public DbSet<DesignRevision> DesignRevisions => Set<DesignRevision>();
	public DbSet<CollaborationStatusEvent> CollaborationStatusEvents => Set<CollaborationStatusEvent>();

	public DbSet<SponsorshipOpportunity> SponsorshipOpportunities => Set<SponsorshipOpportunity>();
	public DbSet<SponsorshipProposal> SponsorshipProposals => Set<SponsorshipProposal>();
	public DbSet<SponsorshipMilestone> SponsorshipMilestones => Set<SponsorshipMilestone>();
	public DbSet<SponsorshipProgressUpdate> SponsorshipProgressUpdates => Set<SponsorshipProgressUpdate>();
	public DbSet<SponsorshipImpactRecord> SponsorshipImpactRecords => Set<SponsorshipImpactRecord>();
	public DbSet<SponsorshipStatusEvent> SponsorshipStatusEvents => Set<SponsorshipStatusEvent>();

	public DbSet<ProductDevelopmentProject> ProductDevelopmentProjects => Set<ProductDevelopmentProject>();
	public DbSet<PrototypeVersion> PrototypeVersions => Set<PrototypeVersion>();
	public DbSet<PrototypeFile> PrototypeFiles => Set<PrototypeFile>();
	public DbSet<ProductDevelopmentComment> ProductDevelopmentComments => Set<ProductDevelopmentComment>();
	public DbSet<ProductDevelopmentMilestone> ProductDevelopmentMilestones => Set<ProductDevelopmentMilestone>();
	public DbSet<ProductDevelopmentStatusEvent> ProductDevelopmentStatusEvents => Set<ProductDevelopmentStatusEvent>();

	public DbSet<InvestmentOpportunity> InvestmentOpportunities => Set<InvestmentOpportunity>();
	public DbSet<InvestmentProposal> InvestmentProposals => Set<InvestmentProposal>();
	public DbSet<InvestmentMilestone> InvestmentMilestones => Set<InvestmentMilestone>();
	public DbSet<InvestmentDocument> InvestmentDocuments => Set<InvestmentDocument>();
	public DbSet<InvestmentStatusEvent> InvestmentStatusEvents => Set<InvestmentStatusEvent>();

	public DbSet<HeritagePlace> HeritagePlaces => Set<HeritagePlace>();
	public DbSet<HeritageFestival> HeritageFestivals => Set<HeritageFestival>();
	public DbSet<CulturalEvent> CulturalEvents => Set<CulturalEvent>();
	public DbSet<LocalCuisine> LocalCuisines => Set<LocalCuisine>();
	public DbSet<HeritageRoute> HeritageRoutes => Set<HeritageRoute>();
	public DbSet<RouteStop> RouteStops => Set<RouteStop>();

	public DbSet<TouristService> TouristServices => Set<TouristService>();
	public DbSet<ServiceAvailabilitySlot> ServiceAvailabilitySlots => Set<ServiceAvailabilitySlot>();
	public DbSet<Booking> Bookings => Set<Booking>();

	public DbSet<LiveClass> LiveClasses => Set<LiveClass>();
	public DbSet<LiveClassParticipant> LiveClassParticipants => Set<LiveClassParticipant>();
	public DbSet<LiveClassQuestion> LiveClassQuestions => Set<LiveClassQuestion>();
	public DbSet<LiveClassAttendance> LiveClassAttendances => Set<LiveClassAttendance>();

	public DbSet<Assignment> Assignments => Set<Assignment>();
	public DbSet<AssignmentSubmission> AssignmentSubmissions => Set<AssignmentSubmission>();
	public DbSet<Quiz> Quizzes => Set<Quiz>();
	public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
	public DbSet<QuizQuestionOption> QuizQuestionOptions => Set<QuizQuestionOption>();
	public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
	public DbSet<QuizAttemptAnswer> QuizAttemptAnswers => Set<QuizAttemptAnswer>();
	public DbSet<Exam> Exams => Set<Exam>();
	public DbSet<ExamQuestion> ExamQuestions => Set<ExamQuestion>();
	public DbSet<ExamQuestionOption> ExamQuestionOptions => Set<ExamQuestionOption>();
	public DbSet<ExamAttempt> ExamAttempts => Set<ExamAttempt>();
	public DbSet<ExamAttemptAnswer> ExamAttemptAnswers => Set<ExamAttemptAnswer>();

	public DbSet<SkillAssessment> SkillAssessments => Set<SkillAssessment>();
	public DbSet<SkillAssessmentInsight> SkillAssessmentInsights => Set<SkillAssessmentInsight>();
	public DbSet<SkillAssessmentRecommendedSkill> SkillAssessmentRecommendedSkills => Set<SkillAssessmentRecommendedSkill>();

	public DbSet<LearningRoadmap> LearningRoadmaps => Set<LearningRoadmap>();
	public DbSet<RoadmapMilestone> RoadmapMilestones => Set<RoadmapMilestone>();
	public DbSet<RoadmapRecommendedCourse> RoadmapRecommendedCourses => Set<RoadmapRecommendedCourse>();
	public DbSet<RoadmapRecommendedLesson> RoadmapRecommendedLessons => Set<RoadmapRecommendedLesson>();

	public DbSet<MuseumItem> MuseumItems => Set<MuseumItem>();
	public DbSet<MuseumItemMedia> MuseumItemMedia => Set<MuseumItemMedia>();
	public DbSet<VillageTourStop> VillageTourStops => Set<VillageTourStop>();
	public DbSet<CulturalStory> CulturalStories => Set<CulturalStory>();
	public DbSet<CulturalStoryChapter> CulturalStoryChapters => Set<CulturalStoryChapter>();

	public DbSet<ResearchProject> ResearchProjects => Set<ResearchProject>();
	public DbSet<ResearchProjectMember> ResearchProjectMembers => Set<ResearchProjectMember>();
	public DbSet<ResearchTask> ResearchTasks => Set<ResearchTask>();
	public DbSet<ResearchMilestone> ResearchMilestones => Set<ResearchMilestone>();
	public DbSet<ResearchNote> ResearchNotes => Set<ResearchNote>();
	public DbSet<ResearchPaper> ResearchPapers => Set<ResearchPaper>();
	public DbSet<ResearchPublication> ResearchPublications => Set<ResearchPublication>();
	public DbSet<ResearchActivity> ResearchActivities => Set<ResearchActivity>();
	public DbSet<ResearchAIAnalysis> ResearchAIAnalyses => Set<ResearchAIAnalysis>();
	public DbSet<ResearchAIFinding> ResearchAIFindings => Set<ResearchAIFinding>();
	public DbSet<ResearchAICitation> ResearchAICitations => Set<ResearchAICitation>();

	public DbSet<HeritageDataset> HeritageDatasets => Set<HeritageDataset>();
	public DbSet<HeritageDatasetVersion> HeritageDatasetVersions => Set<HeritageDatasetVersion>();
	public DbSet<HeritageDatasetAccessGrant> HeritageDatasetAccessGrants => Set<HeritageDatasetAccessGrant>();
	public DbSet<HeritageDatasetExport> HeritageDatasetExports => Set<HeritageDatasetExport>();
	public DbSet<HeritageRiskRecord> HeritageRiskRecords => Set<HeritageRiskRecord>();

	public DbSet<Survey> Surveys => Set<Survey>();
	public DbSet<SurveyQuestion> SurveyQuestions => Set<SurveyQuestion>();
	public DbSet<SurveyFieldAssignment> SurveyFieldAssignments => Set<SurveyFieldAssignment>();
	public DbSet<SurveyResponse> SurveyResponses => Set<SurveyResponse>();
	public DbSet<SurveyResponseAnswer> SurveyResponseAnswers => Set<SurveyResponseAnswer>();
	public DbSet<FieldEvidence> FieldEvidence => Set<FieldEvidence>();
	public DbSet<DataCollectionEvent> DataCollectionEvents => Set<DataCollectionEvent>();

	public DbSet<KnowledgeNode> KnowledgeNodes => Set<KnowledgeNode>();
	public DbSet<KnowledgeRelationship> KnowledgeRelationships => Set<KnowledgeRelationship>();

	public DbSet<InnovationExperiment> InnovationExperiments => Set<InnovationExperiment>();
	public DbSet<InnovationExperimentVersion> InnovationExperimentVersions => Set<InnovationExperimentVersion>();
	public DbSet<TrainingRun> TrainingRuns => Set<TrainingRun>();
	public DbSet<PreservationStrategy> PreservationStrategies => Set<PreservationStrategy>();
	public DbSet<StrategyObjective> StrategyObjectives => Set<StrategyObjective>();
	public DbSet<StrategyAction> StrategyActions => Set<StrategyAction>();
	public DbSet<InnovationPrototype> InnovationPrototypes => Set<InnovationPrototype>();
	public DbSet<PrototypeIteration> PrototypeIterations => Set<PrototypeIteration>();
	public DbSet<PrototypeTestCase> PrototypeTestCases => Set<PrototypeTestCase>();
	public DbSet<PrototypeTestRun> PrototypeTestRuns => Set<PrototypeTestRun>();
	public DbSet<PrototypeTestResult> PrototypeTestResults => Set<PrototypeTestResult>();
	public DbSet<PrototypeIssue> PrototypeIssues => Set<PrototypeIssue>();
	public DbSet<HeritageInnovationSubmission> HeritageInnovationSubmissions => Set<HeritageInnovationSubmission>();
	public DbSet<SubmissionTeamMember> SubmissionTeamMembers => Set<SubmissionTeamMember>();
	public DbSet<SubmissionReview> SubmissionReviews => Set<SubmissionReview>();
	public DbSet<SubmissionEvent> SubmissionEvents => Set<SubmissionEvent>();

	public DbSet<NationalDashboardSnapshot> NationalDashboardSnapshots => Set<NationalDashboardSnapshot>();
	public DbSet<DashboardDistrictStat> DashboardDistrictStats => Set<DashboardDistrictStat>();
	public DbSet<HeritageIndexRecord> HeritageIndexRecords => Set<HeritageIndexRecord>();
	public DbSet<HeritageIndexComponent> HeritageIndexComponents => Set<HeritageIndexComponent>();

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShilpoHubDbContext).Assembly);
		base.OnModelCreating(modelBuilder);
	}
}
