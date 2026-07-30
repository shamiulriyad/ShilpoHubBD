import { Link } from 'react-router-dom';
import { routePaths } from '../routes/routePaths';
import { Button } from '../components/ui';

export default function UnauthorizedPage() {
  return (
    <div className="mx-auto flex min-h-[60vh] max-w-md flex-col items-center justify-center px-4 text-center">
      <p className="text-5xl">🔒</p>
      <h1 className="mt-4 text-xl font-semibold text-heading">Access Restricted</h1>
      <p className="mt-2 text-sm text-body/60">You don't have permission to view this page.</p>
      <Link to={routePaths.home} className="mt-6">
        <Button variant="primary">Back to Home</Button>
      </Link>
    </div>
  );
}
