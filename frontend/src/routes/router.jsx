import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import RootLayout from '../layouts/RootLayout';
import AuthLayout from '../layouts/AuthLayout';
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import DashboardPage from '../pages/DashboardPage';
import AdminPage from '../pages/Admin/AdminPage';
import CustomerPage from '../pages/Customer/CustomerPage';
import VisitorPage from '../pages/Visitor/VisitorPage';
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
import { ProtectedRoute } from './ProtectedRoute';
import { RoleBasedRoute } from './RoleBasedRoute';
import { routePaths } from './routePaths';

const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      { path: routePaths.home, element: <HomePage /> },
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
      { path: routePaths.unauthorized, element: <UnauthorizedPage /> },
      {
        element: <AuthLayout />,
        children: [{ path: routePaths.login, element: <LoginPage /> }],
      },
      {
        element: <ProtectedRoute />,
        children: [
          { path: routePaths.dashboard, element: <DashboardPage /> },
          {
            element: <RoleBasedRoute allowedRoles={["Admin"]} />,
            children: [{ path: routePaths.admin, element: <AdminPage /> }],
          },
        ],
      },
      { path: routePaths.notFound, element: <NotFoundPage /> },
      { path: '/app', element: <Navigate to={routePaths.dashboard} replace /> },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
import { createBrowserRouter, Navigate, RouterProvider } from 'react-router-dom';
import RootLayout from '../layouts/RootLayout';
import AuthLayout from '../layouts/AuthLayout';
import HomePage from '../pages/HomePage';
import LoginPage from '../pages/LoginPage';
import DashboardPage from '../pages/DashboardPage';
import AdminPage from '../pages/AdminPage';
import UnauthorizedPage from '../pages/UnauthorizedPage';
import NotFoundPage from '../pages/NotFoundPage';
import { ProtectedRoute } from './ProtectedRoute';
import { RoleBasedRoute } from './RoleBasedRoute';
import { routePaths } from './routePaths';

const router = createBrowserRouter([
  {
    element: <RootLayout />,
    children: [
      { path: routePaths.home, element: <HomePage /> },
      { path: routePaths.unauthorized, element: <UnauthorizedPage /> },
      {
        element: <AuthLayout />,
        children: [{ path: routePaths.login, element: <LoginPage /> }],
      },
      {
        element: <ProtectedRoute />,
        children: [
          { path: routePaths.dashboard, element: <DashboardPage /> },
          {
            element: <RoleBasedRoute allowedRoles={["Admin"]} />,
            children: [{ path: routePaths.admin, element: <AdminPage /> }],
          },
        ],
      },
      { path: routePaths.notFound, element: <NotFoundPage /> },
      { path: '/app', element: <Navigate to={routePaths.dashboard} replace /> },
    ],
  },
]);

export function AppRouter() {
  return <RouterProvider router={router} />;
}
