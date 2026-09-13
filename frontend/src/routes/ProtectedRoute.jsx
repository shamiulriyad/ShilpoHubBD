import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { routePaths } from './routePaths';
import { useAuth } from '../hooks/useAuth';

export function ProtectedRoute() {
  const { isAuthenticated, isHydrated } = useAuth();
  const location = useLocation();

  // Don't decide anything until the persisted session has been read back,
  // otherwise a page refresh flashes the login screen for a signed-in user.
  if (!isHydrated) {
    return null;
  }

  if (!isAuthenticated) {
    return <Navigate to={routePaths.login} replace state={{ from: location }} />;
  }

  return <Outlet />;
}
