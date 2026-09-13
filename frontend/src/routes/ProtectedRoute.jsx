import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { routePaths } from './routePaths';
import { useAuth } from '../hooks/useAuth';
import FullPageLoader from '../components/ui/FullPageLoader';

export function ProtectedRoute() {
  const { isAuthenticated, isHydrated } = useAuth();
  const location = useLocation();

<<<<<<< HEAD
  if (!isHydrated) {
    return <FullPageLoader label="Checking your session…" />;
=======
  // Don't decide anything until the persisted session has been read back,
  // otherwise a page refresh flashes the login screen for a signed-in user.
  if (!isHydrated) {
    return null;
>>>>>>> 9f65c4a6b6e8629b0fabed739059cc62913ce9c4
  }

  if (!isAuthenticated) {
    return <Navigate to={routePaths.login} replace state={{ from: location }} />;
  }

  return <Outlet />;
}