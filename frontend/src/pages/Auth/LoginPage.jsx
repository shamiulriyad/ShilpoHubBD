import { Link, useLocation, useNavigate } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { useAuth } from '../../contexts/AuthContext';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();

  const handleSubmit = (event) => {
    event.preventDefault();
    // Demo-only UI flow: no real credential verification, just marks the session as logged in.
    login({ name: 'Demo User', role: 'Member', token: 'demo-token' });
    navigate(location.state?.from?.pathname || routePaths.dashboard, { replace: true });
  };

  return (
    <div>
      <h1 className="text-xl font-semibold text-heading">Welcome back</h1>
      <p className="mt-1 text-sm text-body/60">Log in to your ShilpoHub account.</p>

      <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
        <div>
          <label className="mb-1 block text-xs font-medium text-body/70">Email</label>
          <input type="email" placeholder="you@example.com" className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
        </div>
        <div>
          <label className="mb-1 block text-xs font-medium text-body/70">Password</label>
          <input type="password" placeholder="••••••••" className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
        </div>
        <div className="flex items-center justify-between text-xs">
          <label className="flex items-center gap-2 text-body/70">
            <input type="checkbox" className="h-3.5 w-3.5" />
            Remember me
          </label>
          <a href="#" className="text-link hover:underline">
            Forgot password?
          </a>
        </div>
        <Button type="submit" variant="primary" className="w-full">
          Log In
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-body/60">
        Don't have an account?{' '}
        <Link to={routePaths.register} className="font-medium text-link hover:underline">
          Register
        </Link>
      </p>
    </div>
  );
}
