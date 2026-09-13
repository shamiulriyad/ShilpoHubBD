import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { routePaths } from './routePaths';
import { useAuth } from '../hooks/useAuth';
import FullPageLoader from '../components/ui/FullPageLoader';

export function RoleBasedRoute({ allowedRoles = [] }) {
  const { isAuthenticated, isHydrated, hasAnyRole } = useAuth();
  const location = useLocation();

  if (!isHydrated) {
    return <FullPageLoader label="Checking your access…" />;
  }

  if (!isAuthenticated) {
    return <Navigate to={routePaths.login} replace state={{ from: location }} />;
  }

  if (!hasAnyRole(allowedRoles)) {
    return <Navigate to={routePaths.unauthorized} replace state={{ from: location.pathname }} />;
  }

  return <Outlet />;
}
