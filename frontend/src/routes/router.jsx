import PublishedContentDetails from '../pages/News/PublishedContentDetails';
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

import ProducerDashboard from '../pages/Producer/ProducerDashboard';
import ProducerContracts from '../pages/Producer/Contracts';
import ProducerQuotations from '../pages/Producer/Quotations';
import ProducerManufacturingPartnerships from '../pages/Producer/ManufacturingPartnerships';
import ProducerDesignCollaborations from '../pages/Producer/DesignCollaborations';
import ProducerProductDevelopment from '../pages/Producer/ProductDevelopment';
import ProducerCsrSponsorship from '../pages/Producer/CsrSponsorship';
import ProducerInvestmentOpportunities from '../pages/Producer/InvestmentOpportunities';
import ProducerInventory from '../pages/Producer/Inventory';
import ProducerOrders from '../pages/Producer/Orders';
import ProducerProducts from '../pages/Producer/Products';
import ProducerSustainability from '../pages/Producer/Sustainability';
import ProducerLiveShoppingManager from '../pages/Producer/LiveShoppingManager';

import BusinessPartnerDashboard from '../pages/BusinessPartner/BusinessPartnerDashboard';
import BusinessPartnerProfile from '../pages/BusinessPartner/Profile';
import BusinessPartnerContracts from '../pages/BusinessPartner/Contracts';
import BusinessPartnerQuotations from '../pages/BusinessPartner/Quotations';
import BusinessPartnerProcurements from '../pages/BusinessPartner/Procurements';
import BusinessPartnerManufacturingPartnerships from '../pages/BusinessPartner/ManufacturingPartnerships';
import BusinessPartnerDesignCollaborations from '../pages/BusinessPartner/DesignCollaborations';
import BusinessPartnerProductDevelopment from '../pages/BusinessPartner/ProductDevelopment';
import BusinessPartnerSponsorshipMarketplace from '../pages/BusinessPartner/SponsorshipMarketplace';
import BusinessPartnerInvestmentMarketplace from '../pages/BusinessPartner/InvestmentMarketplace';
import BusinessPartnerSupplierDiscovery from '../pages/BusinessPartner/SupplierDiscovery';
import BusinessPartnerSupplierMatching from '../pages/BusinessPartner/SupplierMatching';
import BusinessPartnerProducerComparison from '../pages/BusinessPartner/ProducerComparison';
import BusinessPartnerAnalytics from '../pages/BusinessPartner/Analytics';
import BusinessPartnerAiIntelligence from '../pages/BusinessPartner/AiIntelligence';

import HomePage from '../pages/Home/HomePage';

import ExploreHome from '../pages/Explore/ExploreHome';
import Districts from '../pages/Explore/Districts';
import DistrictDetails from '../pages/Explore/DistrictDetails';
import Villages from '../pages/Explore/Villages';
import VillageDetails from '../pages/Explore/VillageDetails';
import Crafts from '../pages/Explore/Crafts';
import CraftDetails from '../pages/Explore/CraftDetails';
import Producers from '../pages/Explore/Producers';
import ProducerDetails from '../pages/Explore/ProducerDetails';
import Unesco from '../pages/Explore/Unesco';
import DigitalMuseum from '../pages/Explore/DigitalMuseum';

import MarketplaceHome from '../pages/Marketplace/MarketplaceHome';
import ProductListing from '../pages/Marketplace/ProductListing';
import ProductDetails from '../pages/Marketplace/ProductDetails';
import Categories from '../pages/Marketplace/Categories';
import Wishlist from '../pages/Marketplace/Wishlist';
import Cart from '../pages/Marketplace/Cart';
import Checkout from '../pages/Marketplace/Checkout';
import Auctions from '../pages/Marketplace/Auctions';

import TourismHome from '../pages/Tourism/TourismHome';
import HeritageMap from '../pages/Tourism/HeritageMap';
import HeritagePlaceDetails from '../pages/Tourism/HeritagePlaceDetails';
import TourismLocationDetails from '../pages/Tourism/TourismLocationDetails';
import FestivalDirectory from '../pages/Tourism/FestivalDirectory';
import CulturalEvents from '../pages/Tourism/CulturalEvents';
import VillageExplorer from '../pages/Tourism/VillageExplorer';
import TourRoutes from '../pages/Tourism/TourRoutes';
import LocalCuisines from '../pages/Tourism/LocalCuisines';
import TouristServices from '../pages/Tourism/TouristServices';
import TouristServiceDetails from '../pages/Tourism/TouristServiceDetails';
import MyBookings from '../pages/Tourism/MyBookings';
import AiTourismPlanner from '../pages/Tourism/AiTourismPlanner';
import MyTripPlans from '../pages/Tourism/MyTripPlans';
import TravelPassport from '../pages/Tourism/TravelPassport';

import CourseCatalog from '../pages/Academy/CourseCatalog';
import CourseDetails from '../pages/Academy/CourseDetails';
import Mentors from '../pages/Academy/Mentors';
import Certifications from '../pages/Academy/Certifications';
import LearningDashboard from '../pages/Academy/LearningDashboard';
import Certificates from '../pages/Academy/Certificates';
import Portfolio from '../pages/Academy/Portfolio';
import LearningRoadmap from '../pages/Academy/LearningRoadmap';
import MentorshipRequests from '../pages/Academy/MentorshipRequests';
import JobBoard from '../pages/Dashboard/JobBoard';
import MentorMatching from '../pages/Academy/MentorMatching';
import LiveClasses from '../pages/Academy/LiveClasses';
import LiveClassDetails from '../pages/Academy/LiveClassDetails';
import ExamDetails from '../pages/Academy/ExamDetails';
import QuizDetails from '../pages/Academy/QuizDetails';
import AssignmentDetails from '../pages/Academy/AssignmentDetails';
import SkillAssessments from '../pages/Academy/SkillAssessments';

import InnovationHubHome from '../pages/Research/InnovationHubHome';
import ResearchWorkspace from '../pages/Research/ResearchWorkspace';
import ResearchAiAssistant from '../pages/Research/ResearchAiAssistant';
import FieldResearch from '../pages/Research/FieldResearch';
import KnowledgeGraph from '../pages/Research/KnowledgeGraph';
import PreservationStrategies from '../pages/Research/PreservationStrategies';
import InnovationExperiments from '../pages/Research/InnovationExperiments';
import HeritageInnovationSubmissions from '../pages/Research/HeritageInnovationSubmissions';
import InnovationPrototypes from '../pages/Research/InnovationPrototypes';
import ProducerQuestions from '../pages/Producer/Questions';
import ProducerReturns from '../pages/Producer/Returns';
import ProducerProcurements from '../pages/Producer/Procurements';
import ProducerComplaints from '../pages/Producer/Complaints';
import ProducerExpertise from '../pages/Producer/Expertise';
import CustomerComplaints from '../pages/Customer/Complaints';
import ProcurementInspections from '../pages/Admin/ProcurementInspections';
import ProfileApprovals from '../pages/Admin/ProfileApprovals';
import AdminExpertiseCertificates from '../pages/Admin/ExpertiseCertificates';
import Publications from '../pages/Research/Publications';
import HeritageDatabase from '../pages/Research/HeritageDatabase';

import AboutPage from '../pages/About/AboutPage';
import LoginPage from '../pages/Auth/LoginPage';
import RegisterPage from '../pages/Auth/RegisterPage';
import ForgotPasswordPage from '../pages/Auth/ForgotPasswordPage';
import ResetPasswordPage from '../pages/Auth/ResetPasswordPage';

import DashboardHome from '../pages/Dashboard/DashboardHome';
import DashboardExplore from '../pages/Dashboard/DashboardExplore';
import DashboardMarketplace from '../pages/Dashboard/DashboardMarketplace';
import DashboardTourism from '../pages/Dashboard/DashboardTourism';
import DashboardAcademy from '../pages/Dashboard/DashboardAcademy';
import DashboardCommunity from '../pages/Dashboard/DashboardCommunity';
import DashboardMessages from '../pages/Dashboard/DashboardMessages';
import DashboardSettings from '../pages/Dashboard/DashboardSettings';
import DashboardNotifications from '../pages/Dashboard/DashboardNotifications';
import DashboardProfile from '../pages/Dashboard/DashboardProfile';

import AdminWorkspace from '../pages/Admin/AdminWorkspace';

import CustomerDashboard from '../pages/Customer/CustomerDashboard';
import CustomerMarketplace from '../pages/Customer/Marketplace';
import CustomerProductDetails from '../pages/Customer/ProductDetails';
import CraftStory from '../pages/Customer/CraftStory';
import ProducerProfile from '../pages/Customer/ProducerProfile';
import ProducerStory from '../pages/Customer/ProducerStory';
import WorkshopGallery from '../pages/Customer/WorkshopGallery';
import CustomerWishlist from '../pages/Customer/Wishlist';
import ShoppingCart from '../pages/Customer/ShoppingCart';
import CustomerCheckout from '../pages/Customer/Checkout';
import OrderSuccess from '../pages/Customer/OrderSuccess';
import CustomOrder from '../pages/Customer/CustomOrder';
import ProducerCustomOrders from '../pages/Producer/CustomOrders';
import ProducerAuctions from '../pages/Producer/Auctions';
import LiveShopping from '../pages/Customer/LiveShopping';
import AuctionMarketplace from '../pages/Customer/AuctionMarketplace';
import AuctionDetails from '../pages/Customer/AuctionDetails';
import CommunityFeed from '../pages/Customer/CommunityFeed';
import DiscussionForum from '../pages/Customer/DiscussionForum';
import QuestionsAnswers from '../pages/Customer/QuestionsAnswers';
import CustomerMessages from '../pages/Customer/Messages';
import FollowingProducers from '../pages/Customer/FollowingProducers';
import FavoriteVillages from '../pages/Customer/FavoriteVillages';
import OrderHistory from '../pages/Customer/OrderHistory';
import OrderDetails from '../pages/Customer/OrderDetails';
import Returns from '../pages/Customer/Returns';
import Refunds from '../pages/Customer/Refunds';
import HeritageCollection from '../pages/Customer/HeritageCollection';
import PurchaseAnalytics from '../pages/Customer/PurchaseAnalytics';
import ImpactDashboard from '../pages/Customer/ImpactDashboard';
import HeritagePassport from '../pages/Customer/HeritagePassport';
import Achievements from '../pages/Customer/Achievements';
import BadgeCollection from '../pages/Customer/BadgeCollection';
import AIInteriorPreview from '../pages/Customer/AIInteriorPreview';
import AIFashionMatching from '../pages/Customer/AIFashionMatching';
import AIGiftRecommendation from '../pages/Customer/AIGiftRecommendation';
import AISimilarProducts from '../pages/Customer/AISimilarProducts';
import TouristPage from '../pages/Tourist/TouristPage';
import TrainerMasterArtisanPage from '../pages/TrainerMasterArtisan/TrainerMasterArtisanPage';
import ApprenticeshipPrograms from '../pages/TrainerMasterArtisan/ApprenticeshipPrograms';
import MyApprenticeships from '../pages/ApprenticeStudent/MyApprenticeships';
import BrowsePrograms from '../pages/ApprenticeStudent/BrowsePrograms';
import ApprenticeStudentPage from '../pages/ApprenticeStudent/ApprenticeStudentPage';
import GovernmentPage from '../pages/Government/GovernmentPage';
import GovReportsForecasts from '../pages/Government/GovReportsForecasts';
import PolicyCompliance from '../pages/Government/PolicyCompliance';
import ComplaintsMonitoring from '../pages/Government/ComplaintsMonitoring';
import Funding from '../pages/Government/Funding';
import NGOPage from '../pages/NGO/NGOPage';
import ResearcherPage from '../pages/Researcher/ResearcherPage';
import LogisticsPartnerPage from '../pages/LogisticsPartner/LogisticsPartnerPage';
import LogisticsPartnerProfile from '../pages/LogisticsPartner/Profile';
import LogisticsPartnerWarehouses from '../pages/LogisticsPartner/Warehouses';
import LogisticsPartnerShipments from '../pages/LogisticsPartner/Shipments';
import LogisticsPartnerWarehouseStock from '../pages/LogisticsPartner/WarehouseStock';
import LogisticsPartnerPickupRequests from '../pages/LogisticsPartner/PickupRequests';
import LogisticsPartnerReturns from '../pages/LogisticsPartner/Returns';
import LogisticsPartnerDeliveryRoutes from '../pages/LogisticsPartner/DeliveryRoutes';
import LogisticsPartnerAiLogisticsTools from '../pages/LogisticsPartner/AiLogisticsTools';
import LogisticsWorkspaceGuard from '../components/logistics/LogisticsWorkspaceGuard';

import UnauthorizedPage from '../pages/UnauthorizedPage';
import NotFoundPage from '../pages/NotFoundPage';
import RouteErrorPage from '../pages/RouteErrorPage';

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
          { path: routePaths.tourismPassport, element: <TravelPassport /> },
          { path: routePaths.tourismBookings, element: <MyBookings /> },
          { path: routePaths.tourismAiPlanner, element: <AiTourismPlanner /> },
          { path: routePaths.tourismMyPlans, element: <MyTripPlans /> },
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
        element: <RoleBasedRoute allowedRoles={['Tourist', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={touristSidebarNav} sidebarTitle="Tourist" />,
            children: [{ path: routePaths.tourist, element: <TouristPage /> }],
          },
        ],
      },
      {
        element: <RoleBasedRoute allowedRoles={['HeritageAcademyMember', 'SuperAdmin']} />,
        children: [
          {
            element: <DashboardLayout navItems={academyMemberSidebarNav} sidebarTitle="Academy" />,
            children: [{ path: routePaths.academyMember, element: <LearningDashboard /> }],
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
              { path: routePaths.governmentReportsForecasts, element: <GovReportsForecasts /> },
              { path: routePaths.governmentPolicyCompliance, element: <PolicyCompliance /> },
              { path: routePaths.governmentComplaintsMonitoring, element: <ComplaintsMonitoring /> },
              { path: routePaths.governmentFunding, element: <Funding /> },
              { path: routePaths.ngo, element: <NGOPage /> },
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
              { path: routePaths.adminProfileApprovals, element: <ProfileApprovals /> },
              { path: routePaths.adminExpertiseCertificates, element: <AdminExpertiseCertificates /> },
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
          { path: routePaths.producerDesignCollaborations, element: <ProducerDesignCollaborations /> },
          { path: routePaths.producerProductDevelopment, element: <ProducerProductDevelopment /> },
          { path: routePaths.producerCsr, element: <ProducerCsrSponsorship /> },
          { path: routePaths.producerInvestments, element: <ProducerInvestmentOpportunities /> },
          { path: routePaths.producerInventory, element: <ProducerInventory /> },
          { path: routePaths.producerProducts, element: <ProducerProducts /> },
          { path: routePaths.producerOrders, element: <ProducerOrders /> },
          { path: routePaths.producerCustomOrders, element: <ProducerCustomOrders /> },
          { path: routePaths.producerQuestions, element: <ProducerQuestions /> },
          { path: routePaths.producerReturns, element: <ProducerReturns /> },
          { path: routePaths.producerProcurements, element: <ProducerProcurements /> },
          { path: routePaths.producerComplaints, element: <ProducerComplaints /> },
          { path: routePaths.producerExpertise, element: <ProducerExpertise /> },
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
  return <RouterProvider router={router} />;
}
