import { Link, Outlet } from 'react-router-dom';
import { routePaths } from '../routes/routePaths';

export default function AuthLayout() {
  return (
    <div className="flex min-h-screen flex-col bg-background">
      <div className="flex flex-1 flex-col items-center justify-center px-4 py-12">
        <Link to={routePaths.home} className="mb-8 flex items-center gap-2 text-xl font-bold text-title">
          <span className="flex h-9 w-9 items-center justify-center rounded-lg bg-primary text-sm text-surface">
            SH
          </span>
          ShilpoHub
        </Link>
        <div className="w-full max-w-md rounded-2xl border border-border bg-surface p-8 shadow-sm">
          <Outlet />
        </div>
      </div>
    </div>
  );
}
