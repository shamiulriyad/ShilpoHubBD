import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { routePaths } from './routePaths';
import { useAuth } from '../hooks/useAuth';
import FullPageLoader from '../components/ui/FullPageLoader';

/**
 * Gates a branch of the route tree to specific backend roles.
 * `allowedRoles` are exact backend role names (see utils/roles ROLES).
 */
export function RoleBasedRoute({ allowedRoles = [] }) {
  const { isAuthenticated, isHydrated, hasAnyRole } = useAuth();
  const location = useLocation();

  if (!isHydrated) {
<<<<<<< HEAD
    return <FullPageLoader label="Checking your access…" />;
=======
    return null;
>>>>>>> 9f65c4a6b6e8629b0fabed739059cc62913ce9c4
  }

  if (!isAuthenticated) {
    return <Navigate to={routePaths.login} replace state={{ from: location }} />;
  }

  if (!hasAnyRole(allowedRoles)) {
<<<<<<< HEAD
    return <Navigate to={routePaths.unauthorized} replace state={{ from: location.pathname }} />;
=======
    return (
      <Navigate to={routePaths.unauthorized} replace state={{ from: location.pathname }} />
    );
>>>>>>> 9f65c4a6b6e8629b0fabed739059cc62913ce9c4
  }

  return <Outlet />;
}