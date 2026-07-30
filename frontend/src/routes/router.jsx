import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import RootLayout from '../layouts/RootLayout';
import AuthLayout from '../layouts/AuthLayout';
import DashboardLayout from '../layouts/DashboardLayout';
import { ProtectedRoute } from './ProtectedRoute';
import { routePaths } from './routePaths';
import { adminSidebarNav } from '../data/navigation';

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
import FestivalDirectory from '../pages/Tourism/FestivalDirectory';
import CulturalEvents from '../pages/Tourism/CulturalEvents';
import VillageExplorer from '../pages/Tourism/VillageExplorer';
import TourRoutes from '../pages/Tourism/TourRoutes';
import TravelPassport from '../pages/Tourism/TravelPassport';

import CourseCatalog from '../pages/Academy/CourseCatalog';
import CourseDetails from '../pages/Academy/CourseDetails';
import Mentors from '../pages/Academy/Mentors';
import Certifications from '../pages/Academy/Certifications';
import LearningDashboard from '../pages/Academy/LearningDashboard';
import Certificates from '../pages/Academy/Certificates';
import Portfolio from '../pages/Academy/Portfolio';

import InnovationHubHome from '../pages/Research/InnovationHubHome';
import ResearchWorkspace from '../pages/Research/ResearchWorkspace';
import Publications from '../pages/Research/Publications';
import HeritageDatabase from '../pages/Research/HeritageDatabase';

import NewsList from '../pages/News/NewsList';
import NewsDetails from '../pages/News/NewsDetails';
import AboutPage from '../pages/About/AboutPage';
import LoginPage from '../pages/Auth/LoginPage';
import RegisterPage from '../pages/Auth/RegisterPage';

import DashboardHome from '../pages/Dashboard/DashboardHome';
import DashboardExplore from '../pages/Dashboard/DashboardExplore';
import DashboardMarketplace from '../pages/Dashboard/DashboardMarketplace';
import DashboardTourism from '../pages/Dashboard/DashboardTourism';
import DashboardAcademy from '../pages/Dashboard/DashboardAcademy';
import DashboardCommunity from '../pages/Dashboard/DashboardCommunity';
import DashboardAnalytics from '../pages/Dashboard/DashboardAnalytics';
import DashboardMessages from '../pages/Dashboard/DashboardMessages';
import DashboardNotifications from '../pages/Dashboard/DashboardNotifications';
import DashboardSettings from '../pages/Dashboard/DashboardSettings';
import DashboardProfile from '../pages/Dashboard/DashboardProfile';

import AdminDashboard from '../pages/Admin/AdminDashboard';
import UserManagement from '../pages/Admin/UserManagement';
import HeritageManagement from '../pages/Admin/HeritageManagement';
import MarketplaceMonitoring from '../pages/Admin/MarketplaceMonitoring';
import CMS from '../pages/Admin/CMS';
import SecurityCenter from '../pages/Admin/SecurityCenter';

import VisitorPage from '../pages/Visitor/VisitorPage';
import CustomerPage from '../pages/Customer/CustomerPage';
import ArtisanPage from '../pages/Artisan/ArtisanPage';
import FarmerPage from '../pages/Farmer/FarmerPage';
import RetailerPage from '../pages/Retailer/RetailerPage';
import IndustryGarmentsPage from '../pages/IndustryGarments/IndustryGarmentsPage';
import TouristPage from '../pages/Tourist/TouristPage';
import TrainerMasterArtisanPage from '../pages/TrainerMasterArtisan/TrainerMasterArtisanPage';
import ApprenticeStudentPage from '../pages/ApprenticeStudent/ApprenticeStudentPage';
import GovernmentPage from '../pages/Government/GovernmentPage';
import NGOPage from '../pages/NGO/NGOPage';
import ResearcherPage from '../pages/Researcher/ResearcherPage';
import ExporterPage from '../pages/Exporter/ExporterPage';
import LogisticsPartnerPage from '../pages/LogisticsPartner/LogisticsPartnerPage';

import UnauthorizedPage from '../pages/UnauthorizedPage';
import NotFoundPage from '../pages/NotFoundPage';

const router = createBrowserRouter([
  {
    element: <RootLayout />,
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
      { path: routePaths.marketplaceWishlist, element: <Wishlist /> },
      { path: routePaths.marketplaceCart, element: <Cart /> },
      { path: routePaths.marketplaceCheckout, element: <Checkout /> },
      { path: routePaths.marketplaceAuctions, element: <Auctions /> },

      { path: routePaths.tourism, element: <TourismHome /> },
      { path: routePaths.tourismMap, element: <HeritageMap /> },
      { path: routePaths.tourismFestivals, element: <FestivalDirectory /> },
      { path: routePaths.tourismEvents, element: <CulturalEvents /> },
      { path: routePaths.tourismVillages, element: <VillageExplorer /> },
      { path: routePaths.tourismRoutes, element: <TourRoutes /> },
      { path: routePaths.tourismPassport, element: <TravelPassport /> },

      { path: routePaths.academy, element: <CourseCatalog /> },
      { path: routePaths.academyCourseDetails, element: <CourseDetails /> },
      { path: routePaths.academyMentors, element: <Mentors /> },
      { path: routePaths.academyCertifications, element: <Certifications /> },

      { path: routePaths.research, element: <InnovationHubHome /> },
      { path: routePaths.researchWorkspace, element: <ResearchWorkspace /> },
      { path: routePaths.researchPublications, element: <Publications /> },
      { path: routePaths.researchHeritageDatabase, element: <HeritageDatabase /> },

      { path: routePaths.news, element: <NewsList /> },
      { path: routePaths.newsDetails, element: <NewsDetails /> },
      { path: routePaths.about, element: <AboutPage /> },

      { path: routePaths.unauthorized, element: <UnauthorizedPage /> },
      { path: routePaths.notFound, element: <NotFoundPage /> },
    ],
  },

  {
    element: <AuthLayout />,
    children: [
      { path: routePaths.login, element: <LoginPage /> },
      { path: routePaths.register, element: <RegisterPage /> },
    ],
  },

  {
    element: <ProtectedRoute />,
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
          { path: routePaths.dashboardAnalytics, element: <DashboardAnalytics /> },
          { path: routePaths.dashboardMessages, element: <DashboardMessages /> },
          { path: routePaths.dashboardNotifications, element: <DashboardNotifications /> },
          { path: routePaths.dashboardSettings, element: <DashboardSettings /> },
          { path: routePaths.dashboardProfile, element: <DashboardProfile /> },

          { path: routePaths.academyLearning, element: <LearningDashboard /> },
          { path: routePaths.academyCertificates, element: <Certificates /> },
          { path: routePaths.academyPortfolio, element: <Portfolio /> },

          { path: routePaths.visitor, element: <VisitorPage /> },
          { path: routePaths.customer, element: <CustomerPage /> },
          { path: routePaths.artisan, element: <ArtisanPage /> },
          { path: routePaths.farmer, element: <FarmerPage /> },
          { path: routePaths.retailer, element: <RetailerPage /> },
          { path: routePaths.industryGarments, element: <IndustryGarmentsPage /> },
          { path: routePaths.tourist, element: <TouristPage /> },
          { path: routePaths.trainerMasterArtisan, element: <TrainerMasterArtisanPage /> },
          { path: routePaths.apprenticeStudent, element: <ApprenticeStudentPage /> },
          { path: routePaths.government, element: <GovernmentPage /> },
          { path: routePaths.ngo, element: <NGOPage /> },
          { path: routePaths.researcher, element: <ResearcherPage /> },
          { path: routePaths.exporter, element: <ExporterPage /> },
          { path: routePaths.logisticsPartner, element: <LogisticsPartnerPage /> },
        ],
      },
      {
        element: <DashboardLayout navItems={adminSidebarNav} sidebarTitle="Admin" />,
        children: [
          { path: routePaths.admin, element: <AdminDashboard /> },
          { path: routePaths.adminUsers, element: <UserManagement /> },
          { path: routePaths.adminHeritage, element: <HeritageManagement /> },
          { path: routePaths.adminMarketplace, element: <MarketplaceMonitoring /> },
          { path: routePaths.adminCms, element: <CMS /> },
          { path: routePaths.adminSecurity, element: <SecurityCenter /> },
        ],
      },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
