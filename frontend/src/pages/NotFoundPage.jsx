import { Link } from 'react-router-dom';
import { routePaths } from '../routes/routePaths';
import { Button } from '../components/ui';

export default function NotFoundPage() {
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 text-center">
      <p className="text-5xl font-bold text-primary">404</p>
      <h1 className="mt-4 text-xl font-semibold text-heading">Page Not Found</h1>
      <p className="mt-2 text-sm text-body/60">The page you're looking for doesn't exist or has moved.</p>
      <Link to={routePaths.home} className="mt-6">
        <Button variant="primary">Back to Home</Button>
      </Link>
    </div>
  );
}
