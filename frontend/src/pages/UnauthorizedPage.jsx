import { Link, useLocation } from 'react-router-dom';
import { routePaths } from '../routes/routePaths';
import { Button } from '../components/ui';
import { useAuth } from '../hooks/useAuth';
import { roleLabel } from '../utils/roles';

export default function UnauthorizedPage() {
  const location = useLocation();
  const { isAuthenticated, activeRole, homePath } = useAuth();
  const blockedPath = location.state?.from;

  return (
    <div className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 text-center">
      <p className="text-5xl">🔒</p>
      <h1 className="mt-4 text-xl font-semibold text-heading">Access Restricted</h1>
      <p className="mt-2 text-sm text-body/60">
        {blockedPath ? (
          <>
            Your account doesn’t have permission to open{' '}
            <code className="rounded bg-background px-1 py-0.5 text-xs">{blockedPath}</code>.
          </>
        ) : (
          "You don't have permission to view this page."
        )}
        {isAuthenticated && activeRole && (
          <>
            {' '}
            You’re signed in as <span className="font-medium text-body">{roleLabel(activeRole)}</span>.
          </>
        )}
      </p>
      <div className="mt-6 flex flex-wrap items-center justify-center gap-3">
        {isAuthenticated && homePath && (
          <Link to={homePath}>
            <Button variant="primary">Go to My Dashboard</Button>
          </Link>
        )}
        <Link to={routePaths.home}>
          <Button variant={isAuthenticated && homePath ? 'secondary' : 'primary'}>Back to Home</Button>
        </Link>
      </div>
    </div>
  );
}
