import { Link, isRouteErrorResponse, useRouteError } from 'react-router-dom';
import { Button } from '../components/ui';
import { routePaths } from '../routes/routePaths';

export default function RouteErrorPage() {
  const error = useRouteError();
  const status = isRouteErrorResponse(error) ? error.status : null;
  const title = status === 404 ? 'Page not found' : 'This page could not be displayed';
  const message =
    status === 404
      ? 'The page may have moved or the address may be incorrect.'
      : 'An unexpected rendering error occurred. You can return home and try again.';

  return (
    <div className="flex min-h-screen items-center justify-center bg-background px-4">
      <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-8 text-center shadow-sm" role="alert">
        <p className="text-sm font-semibold uppercase tracking-wider text-primary">{status || 'Application error'}</p>
        <h1 className="mt-2 text-2xl font-semibold text-heading">{title}</h1>
        <p className="mt-2 text-sm text-body/60">{message}</p>
        <Link to={routePaths.home} className="mt-6 inline-block">
          <Button variant="primary">Back to Home</Button>
        </Link>
      </div>
    </div>
  );
}
