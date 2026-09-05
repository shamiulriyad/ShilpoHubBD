import { Navigate, Outlet, useLocation } from 'react-router-dom';
import { routePaths } from './routePaths';
import { useAuth } from '../hooks/useAuth';

/**
 * Gates a branch of the route tree to specific backend roles.
 * `allowedRoles` are exact backend role names (see utils/roles ROLES).
 */
export function RoleBasedRoute({ allowedRoles = [] }) {
  const { isAuthenticated, isHydrated, hasAnyRole } = useAuth();
  const location = useLocation();

  if (!isHydrated) {
    return null;
  }

  if (!isAuthenticated) {
    return <Navigate to={routePaths.login} replace state={{ from: location }} />;
  }

  if (!hasAnyRole(allowedRoles)) {
    return (
      <Navigate to={routePaths.unauthorized} replace state={{ from: location.pathname }} />
    );
  }

  return <Outlet />;
}
