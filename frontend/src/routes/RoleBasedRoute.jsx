import { Navigate, Outlet } from 'react-router-dom';
import { routePaths } from './routePaths';
import { useAuth } from '../hooks/useAuth';

export function RoleBasedRoute({ allowedRoles = [] }) {
  const { isAuthenticated, hasRole } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to={routePaths.login} replace />;
  }

  if (allowedRoles.length > 0 && !allowedRoles.some((role) => hasRole(role))) {
    return <Navigate to={routePaths.unauthorized} replace />;
  }

  return <Outlet />;
}
