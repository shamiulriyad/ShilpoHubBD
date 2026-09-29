import { lazy, Suspense } from 'react';
const PublishedContentDetails = lazy(() => import('../pages/News/PublishedContentDetails'));
const ProducerAcademy = lazy(() => import('../pages/Producer/Academy'));
const MentorApplications = lazy(() => import('../pages/Admin/MentorApplications'));
import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import RootLayout from '../layouts/RootLayout';
import AuthLayout from '../layouts/AuthLayout';
import DashboardLayout from '../layouts/DashboardLayout';
import { ProtectedRoute } from './ProtectedRoute';
import { RoleBasedRoute } from './RoleBasedRoute';
import { routePaths } from './routePaths';
import {
  adminSidebarNav,
  customerSidebarNav,
  producerSidebarNav,
  businessPartnerSidebarNav,
  touristSidebarNav,
  academyMemberSidebarNav,
  innovationHubSidebarNav,
  logisticsPartnerSidebarNav,
  governmentNgoSidebarNav,
} from '../data/navigation';

const ProducerDashboard = lazy(() => import('../pages/Producer/ProducerDashboard'));
const ProducerContracts = lazy(() => import('../pages/Producer/Contracts'));
const ProducerQuotations = lazy(() => import('../pages/Producer/Quotations'));
const ProducerManufacturingPartnerships = lazy(() => import('../pages/Producer/ManufacturingPartnerships'));
const ProducerPartnershipAgreements = lazy(() => import('../pages/Producer/PartnershipAgreements'));
const ProducerDesignCollaborations = lazy(() => import('../pages/Producer/DesignCollaborations'));
const ProducerProductDevelopment = lazy(() => import('../pages/Producer/ProductDevelopment'));
const ProducerCsrSponsorship = lazy(() => import('../pages/Producer/CsrSponsorship'));
const ProducerInvestmentOpportunities = lazy(() => import('../pages/Producer/InvestmentOpportunities'));
const ProducerInventory = lazy(() => import('../pages/Producer/Inventory'));
const ProducerOrders = lazy(() => import('../pages/Producer/Orders'));
const ProducerProducts = lazy(() => import('../pages/Producer/Products'));
const ProducerProductAttributes = lazy(() => import('../pages/Producer/ProductAttributes'));
const ProducerProductOverview = lazy(() => import('../pages/Producer/ProductOverview'));
const ProducerSustainability = lazy(() => import('../pages/Producer/Sustainability'));
const ProducerLiveShoppingManager = lazy(() => import('../pages/Producer/LiveShoppingManager'));

const BusinessPartnerDashboard = lazy(() => import('../pages/BusinessPartner/BusinessPartnerDashboard'));
const BusinessPartnerProfile = lazy(() => import('../pages/BusinessPartner/Profile'));
const BusinessPartnerContracts = lazy(() => import('../pages/BusinessPartner/Contracts'));
const BusinessPartnerQuotations = lazy(() => import('../pages/BusinessPartner/Quotations'));
const BusinessPartnerProcurements = lazy(() => import('../pages/BusinessPartner/Procurements'));
const BusinessPartnerManufacturingPartnerships = lazy(() => import('../pages/BusinessPartner/ManufacturingPartnerships'));
const BusinessPartnerDesignCollaborations = lazy(() => import('../pages/BusinessPartner/DesignCollaborations'));
const BusinessPartnerProductDevelopment = lazy(() => import('../pages/BusinessPartner/ProductDevelopment'));
const BusinessPartnerSponsorshipMarketplace = lazy(() => import('../pages/BusinessPartner/SponsorshipMarketplace'));
const BusinessPartnerInvestmentMarketplace = lazy(() => import('../pages/BusinessPartner/InvestmentMarketplace'));
const BusinessPartnerSupplierDiscovery = lazy(() => import('../pages/BusinessPartner/SupplierDiscovery'));
const BusinessPartnerSupplierMatching = lazy(() => import('../pages/BusinessPartner/SupplierMatching'));
const BusinessPartnerProducerComparison = lazy(() => import('../pages/BusinessPartner/ProducerComparison'));
const BusinessPartnerPartnershipAuctions = lazy(() => import('../pages/BusinessPartner/PartnershipAuctions'));
const BusinessPartnerPartnershipAgreements = lazy(() => import('../pages/BusinessPartner/PartnershipAgreements'));
const BusinessPartnerProductIntelligence = lazy(() => import('../pages/BusinessPartner/ProductIntelligence'));
const BusinessPartnerAnalytics = lazy(() => import('../pages/BusinessPartner/Analytics'));
const BusinessPartnerAiIntelligence = lazy(() => import('../pages/BusinessPartner/AiIntelligence'));

const HomePage = lazy(() => import('../pages/Home/HomePage'));

const ExploreHome = lazy(() => import('../pages/Explore/ExploreHome'));
const Districts = lazy(() => import('../pages/Explore/Districts'));
const DistrictDetails = lazy(() => import('../pages/Explore/DistrictDetails'));
const Villages = lazy(() => import('../pages/Explore/Villages'));
const VillageDetails = lazy(() => import('../pages/Explore/VillageDetails'));
const Crafts = lazy(() => import('../pages/Explore/Crafts'));
const CraftDetails = lazy(() => import('../pages/Explore/CraftDetails'));
const Producers = lazy(() => import('../pages/Explore/Producers'));
const ProducerDetails = lazy(() => import('../pages/Explore/ProducerDetails'));
const Unesco = lazy(() => import('../pages/Explore/Unesco'));
const DigitalMuseum = lazy(() => import('../pages/Explore/DigitalMuseum'));

const MarketplaceHome = lazy(() => import('../pages/Marketplace/MarketplaceHome'));
const ProductListing = lazy(() => import('../pages/Marketplace/ProductListing'));
const ProductDetails = lazy(() => import('../pages/Marketplace/ProductDetails'));
const Categories = lazy(() => import('../pages/Marketplace/Categories'));
const Wishlist = lazy(() => import('../pages/Marketplace/Wishlist'));
const Cart = lazy(() => import('../pages/Marketplace/Cart'));
const Checkout = lazy(() => import('../pages/Marketplace/Checkout'));
const Auctions = lazy(() => import('../pages/Marketplace/Auctions'));

const TourismHome = lazy(() => import('../pages/Tourism/TourismHome'));
const HeritageMap = lazy(() => import('../pages/Tourism/HeritageMap'));
const HeritagePlaceDetails = lazy(() => import('../pages/Tourism/HeritagePlaceDetails'));
const TourismLocationDetails = lazy(() => import('../pages/Tourism/TourismLocationDetails'));
const FestivalDirectory = lazy(() => import('../pages/Tourism/FestivalDirectory'));
const CulturalEvents = lazy(() => import('../pages/Tourism/CulturalEvents'));
const VillageExplorer = lazy(() => import('../pages/Tourism/VillageExplorer'));
const TourRoutes = lazy(() => import('../pages/Tourism/TourRoutes'));
const LocalCuisines = lazy(() => import('../pages/Tourism/LocalCuisines'));
const TouristServices = lazy(() => import('../pages/Tourism/TouristServices'));
const TouristServiceDetails = lazy(() => import('../pages/Tourism/TouristServiceDetails'));
const MyBookings = lazy(() => import('../pages/Tourism/MyBookings'));
const AiTourismPlanner = lazy(() => import('../pages/Tourism/AiTourismPlanner'));
const MyTripPlans = lazy(() => import('../pages/Tourism/MyTripPlans'));
const TravelPassport = lazy(() => import('../pages/Tourism/TravelPassport'));

const CourseCatalog = lazy(() => import('../pages/Academy/CourseCatalog'));
const CourseDetails = lazy(() => import('../pages/Academy/CourseDetails'));
const Mentors = lazy(() => import('../pages/Academy/Mentors'));
const Certifications = lazy(() => import('../pages/Academy/Certifications'));
const LearningDashboard = lazy(() => import('../pages/Academy/LearningDashboard'));
const Certificates = lazy(() => import('../pages/Academy/Certificates'));
const Portfolio = lazy(() => import('../pages/Academy/Portfolio'));
const LearningRoadmap = lazy(() => import('../pages/Academy/LearningRoadmap'));
const MentorshipRequests = lazy(() => import('../pages/Academy/MentorshipRequests'));
const JobBoard = lazy(() => import('../pages/Dashboard/JobBoard'));
const MentorMatching = lazy(() => import('../pages/Academy/MentorMatching'));
const LiveClasses = lazy(() => import('../pages/Academy/LiveClasses'));
const LiveClassDetails = lazy(() => import('../pages/Academy/LiveClassDetails'));
const ExamDetails = lazy(() => import('../pages/Academy/ExamDetails'));
const QuizDetails = lazy(() => import('../pages/Academy/QuizDetails'));
const AssignmentDetails = lazy(() => import('../pages/Academy/AssignmentDetails'));
const SkillAssessments = lazy(() => import('../pages/Academy/SkillAssessments'));

const InnovationHubHome = lazy(() => import('../pages/Research/InnovationHubHome'));
const ResearchWorkspace = lazy(() => import('../pages/Research/ResearchWorkspace'));
const ResearchAiAssistant = lazy(() => import('../pages/Research/ResearchAiAssistant'));
const FieldResearch = lazy(() => import('../pages/Research/FieldResearch'));
const KnowledgeGraph = lazy(() => import('../pages/Research/KnowledgeGraph'));
const PreservationStrategies = lazy(() => import('../pages/Research/PreservationStrategies'));
const InnovationExperiments = lazy(() => import('../pages/Research/InnovationExperiments'));
const HeritageInnovationSubmissions = lazy(() => import('../pages/Research/HeritageInnovationSubmissions'));
const InnovationPrototypes = lazy(() => import('../pages/Research/InnovationPrototypes'));
const ProducerQuestions = lazy(() => import('../pages/Producer/Questions'));
const ProducerReturns = lazy(() => import('../pages/Producer/Returns'));
const ProducerProcurements = lazy(() => import('../pages/Producer/Procurements'));
const ProducerComplaints = lazy(() => import('../pages/Producer/Complaints'));
const ProducerExpertise = lazy(() => import('../pages/Producer/Expertise'));
const ProducerSupportCases = lazy(() => import('../pages/Producer/SupportCases'));
const CustomerComplaints = lazy(() => import('../pages/Customer/Complaints'));
const ProcurementInspections = lazy(() => import('../pages/Admin/ProcurementInspections'));
const ProducerPartnershipAuctions = lazy(() => import('../pages/Admin/ProducerPartnershipAuctions'));
const AdminProducerPartnershipAgreements = lazy(() => import('../pages/Admin/ProducerPartnershipAgreements'));
const AdminProducerPartnershipSettlements = lazy(() => import('../pages/Admin/ProducerPartnershipSettlements'));
const ProfileApprovals = lazy(() => import('../pages/Admin/ProfileApprovals'));
const AdminExpertiseCertificates = lazy(() => import('../pages/Admin/ExpertiseCertificates'));
const SupportOversight = lazy(() => import('../pages/Admin/SupportOversight'));
const ProducerIntelligenceDashboard = lazy(() => import('../pages/Admin/ProducerIntelligenceDashboard'));
const ProducerIntelligenceDetail = lazy(() => import('../pages/Admin/ProducerIntelligenceDetail'));
const Publications = lazy(() => import('../pages/Research/Publications'));
const HeritageDatabase = lazy(() => import('../pages/Research/HeritageDatabase'));

const AboutPage = lazy(() => import('../pages/About/AboutPage'));
const LoginPage = lazy(() => import('../pages/Auth/LoginPage'));
const RegisterPage = lazy(() => import('../pages/Auth/RegisterPage'));
const ForgotPasswordPage = lazy(() => import('../pages/Auth/ForgotPasswordPage'));
const ResetPasswordPage = lazy(() => import('../pages/Auth/ResetPasswordPage'));

const DashboardHome = lazy(() => import('../pages/Dashboard/DashboardHome'));
const DashboardExplore = lazy(() => import('../pages/Dashboard/DashboardExplore'));
const DashboardMarketplace = lazy(() => import('../pages/Dashboard/DashboardMarketplace'));
const DashboardTourism = lazy(() => import('../pages/Dashboard/DashboardTourism'));
const DashboardAcademy = lazy(() => import('../pages/Dashboard/DashboardAcademy'));
const DashboardCommunity = lazy(() => import('../pages/Dashboard/DashboardCommunity'));
const DashboardMessages = lazy(() => import('../pages/Dashboard/DashboardMessages'));
const DashboardSettings = lazy(() => import('../pages/Dashboard/DashboardSettings'));
const DashboardNotifications = lazy(() => import('../pages/Dashboard/DashboardNotifications'));
const DashboardProfile = lazy(() => import('../pages/Dashboard/DashboardProfile'));

const AdminWorkspace = lazy(() => import('../pages/Admin/AdminWorkspace'));

const CustomerDashboard = lazy(() => import('../pages/Customer/CustomerDashboard'));
const CustomerMarketplace = lazy(() => import('../pages/Customer/Marketplace'));
const CustomerProductDetails = lazy(() => import('../pages/Customer/ProductDetails'));
const CraftStory = lazy(() => import('../pages/Customer/CraftStory'));
const ProducerProfile = lazy(() => import('../pages/Customer/ProducerProfile'));
const ProducerStory = lazy(() => import('../pages/Customer/ProducerStory'));
const WorkshopGallery = lazy(() => import('../pages/Customer/WorkshopGallery'));
const CustomerWishlist = lazy(() => import('../pages/Customer/Wishlist'));
const ShoppingCart = lazy(() => import('../pages/Customer/ShoppingCart'));
const CustomerCheckout = lazy(() => import('../pages/Customer/Checkout'));
const OrderSuccess = lazy(() => import('../pages/Customer/OrderSuccess'));
const CustomOrder = lazy(() => import('../pages/Customer/CustomOrder'));
const ProducerCustomOrders = lazy(() => import('../pages/Producer/CustomOrders'));
const ProducerAuctions = lazy(() => import('../pages/Producer/Auctions'));
const LiveShopping = lazy(() => import('../pages/Customer/LiveShopping'));
const AuctionMarketplace = lazy(() => import('../pages/Customer/AuctionMarketplace'));
const AuctionDetails = lazy(() => import('../pages/Customer/AuctionDetails'));
const CommunityFeed = lazy(() => import('../pages/Customer/CommunityFeed'));
const DiscussionForum = lazy(() => import('../pages/Customer/DiscussionForum'));
const QuestionsAnswers = lazy(() => import('../pages/Customer/QuestionsAnswers'));
const CustomerMessages = lazy(() => import('../pages/Customer/Messages'));
const FollowingProducers = lazy(() => import('../pages/Customer/FollowingProducers'));
const FavoriteVillages = lazy(() => import('../pages/Customer/FavoriteVillages'));
const OrderHistory = lazy(() => import('../pages/Customer/OrderHistory'));
const OrderDetails = lazy(() => import('../pages/Customer/OrderDetails'));
const Returns = lazy(() => import('../pages/Customer/Returns'));
const Refunds = lazy(() => import('../pages/Customer/Refunds'));
const HeritageCollection = lazy(() => import('../pages/Customer/HeritageCollection'));
const PurchaseAnalytics = lazy(() => import('../pages/Customer/PurchaseAnalytics'));
const ImpactDashboard = lazy(() => import('../pages/Customer/ImpactDashboard'));
const HeritagePassport = lazy(() => import('../pages/Customer/HeritagePassport'));
const Achievements = lazy(() => import('../pages/Customer/Achievements'));
const BadgeCollection = lazy(() => import('../pages/Customer/BadgeCollection'));
const AIInteriorPreview = lazy(() => import('../pages/Customer/AIInteriorPreview'));
const AIFashionMatching = lazy(() => import('../pages/Customer/AIFashionMatching'));
const AIGiftRecommendation = lazy(() => import('../pages/Customer/AIGiftRecommendation'));
const AISimilarProducts = lazy(() => import('../pages/Customer/AISimilarProducts'));
const TouristPage = lazy(() => import('../pages/Tourist/TouristPage'));
const TrainerMasterArtisanPage = lazy(() => import('../pages/TrainerMasterArtisan/TrainerMasterArtisanPage'));
const ApprenticeshipPrograms = lazy(() => import('../pages/TrainerMasterArtisan/ApprenticeshipPrograms'));
const MyApprenticeships = lazy(() => import('../pages/ApprenticeStudent/MyApprenticeships'));
const BrowsePrograms = lazy(() => import('../pages/ApprenticeStudent/BrowsePrograms'));
const ApprenticeStudentPage = lazy(() => import('../pages/ApprenticeStudent/ApprenticeStudentPage'));
const GovernmentPage = lazy(() => import('../pages/Government/GovernmentPage'));
const OrganizationProfile = lazy(() => import('../pages/Government/OrganizationProfile'));
const ArtisanSupportCases = lazy(() => import('../pages/Government/ArtisanSupportCases'));
const GovProducerDashboard = lazy(() => import('../pages/Government/GovProducerDashboard'));
const GovReportsForecasts = lazy(() => import('../pages/Government/GovReportsForecasts'));
const PolicyCompliance = lazy(() => import('../pages/Government/PolicyCompliance'));
const ComplaintsMonitoring = lazy(() => import('../pages/Government/ComplaintsMonitoring'));
const Funding = lazy(() => import('../pages/Government/Funding'));
const NGOPage = lazy(() => import('../pages/NGO/NGOPage'));
const ResearcherPage = lazy(() => import('../pages/Researcher/ResearcherPage'));
const LogisticsPartnerPage = lazy(() => import('../pages/LogisticsPartner/LogisticsPartnerPage'));
const LogisticsPartnerProfile = lazy(() => import('../pages/LogisticsPartner/Profile'));
const LogisticsPartnerWarehouses = lazy(() => import('../pages/LogisticsPartner/Warehouses'));
const LogisticsPartnerShipments = lazy(() => import('../pages/LogisticsPartner/Shipments'));
const LogisticsPartnerWarehouseStock = lazy(() => import('../pages/LogisticsPartner/WarehouseStock'));
const LogisticsPartnerPickupRequests = lazy(() => import('../pages/LogisticsPartner/PickupRequests'));
const LogisticsPartnerReturns = lazy(() => import('../pages/LogisticsPartner/Returns'));
const LogisticsPartnerDeliveryRoutes = lazy(() => import('../pages/LogisticsPartner/DeliveryRoutes'));
const LogisticsPartnerAiLogisticsTools = lazy(() => import('../pages/LogisticsPartner/AiLogisticsTools'));
import LogisticsWorkspaceGuard from '../components/logistics/LogisticsWorkspaceGuard';

const UnauthorizedPage = lazy(() => import('../pages/UnauthorizedPage'));
const NotFoundPage = lazy(() => import('../pages/NotFoundPage'));
const RouteErrorPage = lazy(() => import('../pages/RouteErrorPage'));

const router = createBrowserRouter([
  {
    element: <RootLayout />,
    errorElement: <RouteErrorPage />,
    children: [
      { path: routePaths.home, element: <HomePage /> },

      { path: routePaths.explore, element: <ExploreHome /> },
      { path: routePaths.exploreDistricts, element: <Districts /> },
      { path: routePaths.exploreDistrictDetails, element: <DistrictDetails /> },
      { path: routePaths.exploreVillages, element: <Villages /> },
      { path: routePaths.exploreVillageDetails, element: <VillageDetails /> },
      { path: routePaths.exploreCrafts, element: <Crafts /> },
      { path: routePaths.exploreCraftDetails, element: <CraftDetails /> },
      { path: routePaths.exploreProducers, element: <Producers /> },
      { path: routePaths.exploreProducerDetails, element: <ProducerDetails /> },
      { path: routePaths.exploreUnesco, element: <Unesco /> },
      { path: routePaths.exploreMuseum, element: <DigitalMuseum /> },

      { path: routePaths.marketplace, element: <MarketplaceHome /> },
      { path: routePaths.marketplaceProducts, element: <ProductListing /> },
      { path: routePaths.marketplaceProductDetails, element: <ProductDetails /> },
      { path: routePaths.marketplaceCategories, element: <Categories /> },
      { path: routePaths.marketplaceAuctions, element: <Auctions /> },

      { path: routePaths.tourism, element: <TourismHome /> },
      { path: routePaths.tourismMap, element: <HeritageMap /> },
      { path: routePaths.tourismPlaceDetails, element: <HeritagePlaceDetails /> },
      { path: routePaths.tourismLocationDetails, element: <TourismLocationDetails /> },
      { path: routePaths.tourismFestivals, element: <FestivalDirectory /> },
      { path: routePaths.tourismEvents, element: <CulturalEvents /> },
      { path: "/updates/:kind/:id", element: <PublishedContentDetails /> },
      { path: routePaths.tourismVillages, element: <VillageExplorer /> },
      { path: routePaths.tourismRoutes, element: <TourRoutes /> },
      { path: routePaths.tourismCuisines, element: <LocalCuisines /> },
      { path: routePaths.tourismServices, element: <TouristServices /> },
      { path: routePaths.tourismServiceDetails, element: <TouristServiceDetails /> },

      { path: routePaths.academy, element: <CourseCatalog /> },
      { path: routePaths.academyCourseDetails, element: <CourseDetails /> },
      { path: routePaths.academyMentors, element: <Mentors /> },
      { path: routePaths.academyCertifications, element: <Certifications /> },
      { path: routePaths.academyLiveClasses, element: <LiveClasses /> },
      { path: routePaths.academyLiveClassDetails, element: <LiveClassDetails /> },

      { path: routePaths.research, element: <InnovationHubHome /> },

      { path: routePaths.about, element: <AboutPage /> },


      {
        element: <ProtectedRoute />,
        children: [
          { path: routePaths.marketplaceWishlist, element: <Wishlist /> },
          { path: routePaths.marketplaceCart, element: <Cart /> },
          { path: routePaths.marketplaceCheckout, element: <Checkout /> },
          { path: routePaths.researchWorkspace, element: <ResearchWorkspace /> },
          { path: routePaths.researchAiAssistant, element: <ResearchAiAssistant /> },
          { path: routePaths.researchFieldResearch, element: <FieldResearch /> },
          { path: routePaths.innovationPreservationStrategies, element: <PreservationStrategies /> },
          { path: routePaths.innovationExperiments, element: <InnovationExperiments /> },
          { path: routePaths.innovationSubmissions, element: <HeritageInnovationSubmissions /> },
          { path: routePaths.innovationPrototypes, element: <InnovationPrototypes /> },
          { path: routePaths.researchPublications, element: <Publications /> },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['HeritageInnovationHub', 'GovernmentNGO', 'SuperAdmin']} />,
        children: [
          { path: routePaths.researchKnowledgeGraph, element: <KnowledgeGraph /> },
          { path: routePaths.researchHeritageDatabase, element: <HeritageDatabase /> },
        ],
      },

      { path: routePaths.unauthorized, element: <UnauthorizedPage /> },
      { path: routePaths.notFound, element: <NotFoundPage /> },
    ],
  },

  {
    element: <AuthLayout />,
    errorElement: <RouteErrorPage />,
    children: [
      { path: routePaths.login, element: <LoginPage /> },
      { path: routePaths.register, element: <RegisterPage /> },
      { path: routePaths.forgotPassword, element: <ForgotPasswordPage /> },
      { path: routePaths.resetPassword, element: <ResetPasswordPage /> },
    ],
  },

  {
    element: <ProtectedRoute />,
    errorElement: <RouteErrorPage />,
    children: [
      {
        element: <DashboardLayout />,
        children: [
          { path: routePaths.dashboard, element: <DashboardHome /> },
          { path: routePaths.dashboardExplore, element: <DashboardExplore /> },
          { path: routePaths.dashboardMarketplace, element: <DashboardMarketplace /> },
          { path: routePaths.dashboardTourism, element: <DashboardTourism /> },
          { path: routePaths.dashboardAcademy, element: <DashboardAcademy /> },
          { path: routePaths.dashboardCommunity, element: <DashboardCommunity /> },
          { path: routePaths.dashboardMessages, element: <DashboardMessages /> },
          { path: routePaths.dashboardSettings, element: <DashboardSettings /> },
          { path: '/dashboard/notifications', element: <DashboardNotifications /> },
          { path: routePaths.dashboardProfile, element: <DashboardProfile /> },

          { path: routePaths.academyLearning, element: <LearningDashboard /> },
          { path: routePaths.academyCertificates, element: <Certificates /> },
          { path: routePaths.academyPortfolio, element: <Portfolio /> },
          { path: routePaths.academyRoadmap, element: <LearningRoadmap /> },
          { path: routePaths.academyMentorshipRequests, element: <MentorshipRequests /> },
          { path: routePaths.dashboardJobs, element: <JobBoard /> },
          { path: routePaths.academyMentorMatching, element: <MentorMatching /> },
          { path: routePaths.academyExamDetails, element: <ExamDetails /> },
          { path: routePaths.academyQuizDetails, element: <QuizDetails /> },
          { path: routePaths.academyAssignmentDetails, element: <AssignmentDetails /> },
          { path: routePaths.academySkillAssessments, element: <SkillAssessments /> },

          { path: routePaths.trainerMasterArtisan, element: <TrainerMasterArtisanPage /> },
          { path: routePaths.trainerMasterArtisanPrograms, element: <ApprenticeshipPrograms /> },
          { path: routePaths.apprenticeStudent, element: <ApprenticeStudentPage /> },
          { path: routePaths.apprenticeStudentMyApprenticeships, element: <MyApprenticeships /> },
          { path: routePaths.apprenticeStudentBrowsePrograms, element: <BrowsePrograms /> },
        ],
      },
      {
        // Personal trip tools: only travellers (and admins) plan trips, book services and keep a passport.
        element: <RoleBasedRoute allowedRoles={['Tourist', 'SuperAdmin']} />,
        children: [
          { path: routePaths.tourismPassport, element: <TravelPassport /> },
          { path: routePaths.tourismBookings, element: <MyBookings /> },
          { path: routePaths.tourismAiPlanner, element: <AiTourismPlanner /> },
          { path: routePaths.tourismMyPlans, element: <MyTripPlans /> },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['Tourist', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={touristSidebarNav} sidebarTitle="Tourist" />,
            children: [
              { path: routePaths.tourist, element: <TouristPage /> },
              { path: routePaths.workspaceTouristCrafts, element: <Crafts /> },
              { path: routePaths.workspaceTouristMap, element: <HeritageMap /> },
              { path: routePaths.workspaceTouristFestivals, element: <FestivalDirectory /> },
              { path: routePaths.workspaceTouristEvents, element: <CulturalEvents /> },
              { path: routePaths.workspaceTouristRoutes, element: <TourRoutes /> },
              { path: routePaths.workspaceTouristVillages, element: <VillageExplorer /> },
              { path: routePaths.workspaceTouristCuisines, element: <LocalCuisines /> },
              { path: routePaths.workspaceTouristServices, element: <TouristServices /> },
              { path: routePaths.workspaceTouristBookings, element: <MyBookings /> },
              { path: routePaths.workspaceTouristPassport, element: <TravelPassport /> },
              { path: routePaths.workspaceTouristAiPlanner, element: <AiTourismPlanner /> },
            ],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['HeritageAcademyMember', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={academyMemberSidebarNav} sidebarTitle="Academy" />,
            children: [
              { path: routePaths.academyMember, element: <LearningDashboard /> },
              { path: routePaths.workspaceAcademyCatalog, element: <CourseCatalog /> },
              { path: routePaths.workspaceAcademyMentors, element: <Mentors /> },
              { path: routePaths.workspaceAcademyLiveClasses, element: <LiveClasses /> },
              { path: routePaths.workspaceAcademyCertifications, element: <Certifications /> },
            ],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['HeritageInnovationHub', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={innovationHubSidebarNav} sidebarTitle="Innovation Hub" />,
            children: [{ path: routePaths.researcher, element: <ResearcherPage /> }],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['LogisticsPartner', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={logisticsPartnerSidebarNav} sidebarTitle="Logistics" />,
            children: [{
              element: <LogisticsWorkspaceGuard />,
              children: [
                { path: routePaths.logisticsPartner, element: <LogisticsPartnerPage /> },
                { path: routePaths.logisticsPartnerProfile, element: <LogisticsPartnerProfile /> },
                { path: routePaths.logisticsPartnerWarehouses, element: <LogisticsPartnerWarehouses /> },
                { path: routePaths.logisticsPartnerShipments, element: <LogisticsPartnerShipments /> },
                { path: routePaths.logisticsPartnerStock, element: <LogisticsPartnerWarehouseStock /> },
                { path: routePaths.logisticsPartnerPickups, element: <LogisticsPartnerPickupRequests /> },
                { path: routePaths.logisticsPartnerReturns, element: <LogisticsPartnerReturns /> },
                { path: routePaths.logisticsPartnerRoutes, element: <LogisticsPartnerDeliveryRoutes /> },
                { path: routePaths.logisticsPartnerAiTools, element: <LogisticsPartnerAiLogisticsTools /> },
              ],
            }],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['GovernmentNGO', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={governmentNgoSidebarNav} sidebarTitle="Government & NGO" />,
            children: [
              { path: routePaths.government, element: <GovernmentPage /> },
              { path: routePaths.governmentOrganizationProfile, element: <OrganizationProfile /> },
              { path: routePaths.governmentArtisanSupport, element: <ArtisanSupportCases /> },
              { path: routePaths.governmentProducerDashboard, element: <GovProducerDashboard /> },
              { path: routePaths.governmentReportsForecasts, element: <GovReportsForecasts /> },
              { path: routePaths.governmentPolicyCompliance, element: <PolicyCompliance /> },
              { path: routePaths.governmentComplaintsMonitoring, element: <ComplaintsMonitoring /> },
              { path: routePaths.governmentFunding, element: <Funding /> },
              { path: routePaths.ngo, element: <NGOPage /> },
              { path: routePaths.governmentKnowledgeGraph, element: <KnowledgeGraph /> },
              { path: routePaths.governmentHeritageDatabase, element: <HeritageDatabase /> },
            ],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={adminSidebarNav} sidebarTitle="Admin" />,
            children: [
              { path: routePaths.admin, element: <AdminWorkspace /> },
              { path: routePaths.adminUsers, element: <AdminWorkspace section="users" /> },
              { path: routePaths.adminHeritage, element: <AdminWorkspace section="heritage" /> },
              { path: routePaths.adminMarketplace, element: <AdminWorkspace section="marketplace" /> },
              { path: routePaths.adminProcurementInspections, element: <ProcurementInspections /> },
              { path: routePaths.adminProducerPartnershipAuctions, element: <ProducerPartnershipAuctions /> },
              { path: routePaths.adminProducerPartnershipAgreements, element: <AdminProducerPartnershipAgreements /> },
              { path: routePaths.adminProducerPartnershipSettlements, element: <AdminProducerPartnershipSettlements /> },
              { path: routePaths.adminProfileApprovals, element: <ProfileApprovals /> },
              { path: '/admin/mentor-applications', element: <MentorApplications /> },
              { path: routePaths.adminExpertiseCertificates, element: <AdminExpertiseCertificates /> },
              { path: routePaths.adminSupportOversight, element: <SupportOversight /> },
              { path: routePaths.adminProducerIntelligence, element: <ProducerIntelligenceDashboard /> },
              { path: routePaths.adminProducerIntelligenceDetail, element: <ProducerIntelligenceDetail /> },
              { path: "/admin/:section/:view", element: <AdminWorkspace /> },
              { path: "/admin/:section", element: <AdminWorkspace /> },
            ],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['Customer', 'SuperAdmin']} />,
        children: [
      {
        element: <DashboardLayout navItems={customerSidebarNav} sidebarTitle="Customer" />,
        children: [
          { path: routePaths.customer, element: <CustomerDashboard /> },
          { path: routePaths.customerMarketplace, element: <CustomerMarketplace /> },
          { path: routePaths.customerProductDetails, element: <CustomerProductDetails /> },
          { path: routePaths.customerCraftStory, element: <CraftStory /> },
          { path: routePaths.customerProducerProfile, element: <ProducerProfile /> },
          { path: routePaths.customerProducerStory, element: <ProducerStory /> },
          { path: routePaths.customerWorkshops, element: <WorkshopGallery /> },
          { path: routePaths.customerWishlist, element: <CustomerWishlist /> },
          { path: routePaths.customerCart, element: <ShoppingCart /> },
          { path: routePaths.customerCheckout, element: <CustomerCheckout /> },
          { path: routePaths.customerOrderSuccess, element: <OrderSuccess /> },
          { path: routePaths.customerCustomOrder, element: <CustomOrder /> },
          { path: routePaths.customerLiveShopping, element: <LiveShopping /> },
          { path: routePaths.customerAuctions, element: <AuctionMarketplace /> },
          { path: routePaths.customerAuctionDetails, element: <AuctionDetails /> },
          { path: routePaths.customerCommunity, element: <CommunityFeed /> },
          { path: routePaths.customerForum, element: <DiscussionForum /> },
          { path: routePaths.customerQA, element: <QuestionsAnswers /> },
          { path: routePaths.customerComplaints, element: <CustomerComplaints /> },
          { path: routePaths.customerMessages, element: <CustomerMessages /> },
          { path: routePaths.customerFollowing, element: <FollowingProducers /> },
          { path: routePaths.customerFavoriteVillages, element: <FavoriteVillages /> },
          { path: routePaths.customerOrders, element: <OrderHistory /> },
          { path: routePaths.customerOrderDetails, element: <OrderDetails /> },
          { path: routePaths.customerReturns, element: <Returns /> },
          { path: routePaths.customerRefunds, element: <Refunds /> },
          { path: routePaths.customerHeritageCollection, element: <HeritageCollection /> },
          { path: routePaths.customerPurchaseAnalytics, element: <PurchaseAnalytics /> },
          { path: routePaths.customerImpactDashboard, element: <ImpactDashboard /> },
          { path: routePaths.customerHeritagePassport, element: <HeritagePassport /> },
          { path: routePaths.customerAchievements, element: <Achievements /> },
          { path: routePaths.customerBadges, element: <BadgeCollection /> },
          { path: routePaths.customerAIInteriorPreview, element: <AIInteriorPreview /> },
          { path: routePaths.customerAIFashionMatching, element: <AIFashionMatching /> },
          { path: routePaths.customerAIGiftRecommendation, element: <AIGiftRecommendation /> },
          { path: routePaths.customerAISimilarProducts, element: <AISimilarProducts /> },
        ],
      },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['Producer', 'SuperAdmin']} />,
        children: [
      {
        element: <DashboardLayout navItems={producerSidebarNav} sidebarTitle="Producer" />,
        children: [
          { path: routePaths.producer, element: <ProducerDashboard /> },
          { path: routePaths.producerContracts, element: <ProducerContracts /> },
          { path: routePaths.producerQuotations, element: <ProducerQuotations /> },
          { path: routePaths.producerPartnerships, element: <ProducerManufacturingPartnerships /> },
          { path: routePaths.producerPartnershipAgreements, element: <ProducerPartnershipAgreements /> },
          { path: routePaths.producerDesignCollaborations, element: <ProducerDesignCollaborations /> },
          { path: routePaths.producerProductDevelopment, element: <ProducerProductDevelopment /> },
          { path: routePaths.producerCsr, element: <ProducerCsrSponsorship /> },
          { path: routePaths.producerInvestments, element: <ProducerInvestmentOpportunities /> },
          { path: routePaths.producerInventory, element: <ProducerInventory /> },
          { path: routePaths.producerProducts, element: <ProducerProducts /> },
          { path: routePaths.producerProductAttributes, element: <ProducerProductAttributes /> },
          { path: routePaths.producerProductDetails, element: <ProducerProductOverview /> },
          { path: routePaths.producerOrders, element: <ProducerOrders /> },
          { path: routePaths.producerCustomOrders, element: <ProducerCustomOrders /> },
          { path: routePaths.producerQuestions, element: <ProducerQuestions /> },
          { path: routePaths.producerReturns, element: <ProducerReturns /> },
          { path: routePaths.producerProcurements, element: <ProducerProcurements /> },
          { path: routePaths.producerComplaints, element: <ProducerComplaints /> },
          { path: '/producer/academy', element: <ProducerAcademy /> },
          { path: routePaths.producerExpertise, element: <ProducerExpertise /> },
          { path: routePaths.producerSupportCases, element: <ProducerSupportCases /> },
          { path: routePaths.producerAuctions, element: <ProducerAuctions /> },
          { path: routePaths.producerSustainability, element: <ProducerSustainability /> },
          { path: routePaths.producerLiveShopping, element: <ProducerLiveShoppingManager /> },
        ],
      },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['BusinessPartner', 'SuperAdmin']} />,
        children: [
      {
        element: <DashboardLayout navItems={businessPartnerSidebarNav} sidebarTitle="Business Partner" />,
        children: [
          { path: routePaths.businessPartner, element: <BusinessPartnerDashboard /> },
          { path: routePaths.businessPartnerProfile, element: <BusinessPartnerProfile /> },
          { path: routePaths.businessPartnerContracts, element: <BusinessPartnerContracts /> },
          { path: routePaths.businessPartnerQuotations, element: <BusinessPartnerQuotations /> },
          { path: routePaths.businessPartnerProcurements, element: <BusinessPartnerProcurements /> },
          { path: routePaths.businessPartnerPartnerships, element: <BusinessPartnerManufacturingPartnerships /> },
          { path: routePaths.businessPartnerDesignCollaborations, element: <BusinessPartnerDesignCollaborations /> },
          { path: routePaths.businessPartnerProductDevelopment, element: <BusinessPartnerProductDevelopment /> },
          { path: routePaths.businessPartnerCsr, element: <BusinessPartnerSponsorshipMarketplace /> },
          { path: routePaths.businessPartnerInvestments, element: <BusinessPartnerInvestmentMarketplace /> },
          { path: routePaths.businessPartnerSupplierDiscovery, element: <BusinessPartnerSupplierDiscovery /> },
          { path: routePaths.businessPartnerSupplierMatching, element: <BusinessPartnerSupplierMatching /> },
          { path: routePaths.businessPartnerProducerComparison, element: <BusinessPartnerProducerComparison /> },
          { path: routePaths.businessPartnerPartnershipAuctions, element: <BusinessPartnerPartnershipAuctions /> },
          { path: routePaths.businessPartnerPartnershipAgreements, element: <BusinessPartnerPartnershipAgreements /> },
          { path: routePaths.businessPartnerProductIntelligence, element: <BusinessPartnerProductIntelligence /> },
          { path: routePaths.businessPartnerAnalytics, element: <BusinessPartnerAnalytics /> },
          { path: routePaths.businessPartnerAiIntelligence, element: <BusinessPartnerAiIntelligence /> },
        ],
      },
        ],
      },
    ],
  },
]);

export function AppRouter() {
  return <Suspense fallback={<div role="status" className="p-8">Loading page…</div>}><RouterProvider router={router} /></Suspense>;
}
