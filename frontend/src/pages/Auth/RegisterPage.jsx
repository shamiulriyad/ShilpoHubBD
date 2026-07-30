import { Link, useNavigate } from 'react-router-dom';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { useAuth } from '../../contexts/AuthContext';

const roles = ['Customer', 'Artisan', 'Farmer', 'Tourist', 'Business Partner', 'Researcher', 'NGO', 'Government'];

export default function RegisterPage() {
  const { login } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = (event) => {
    event.preventDefault();
    // Demo-only UI flow: no real account creation, just marks the session as logged in.
    login({ name: 'New Member', role: 'Member', token: 'demo-token' });
    navigate(routePaths.dashboard, { replace: true });
  };

  return (
    <div>
      <h1 className="text-xl font-semibold text-heading">Create your account</h1>
      <p className="mt-1 text-sm text-body/60">Join the ShilpoHub heritage ecosystem.</p>

      <form className="mt-6 space-y-4" onSubmit={handleSubmit}>
        <div>
          <label className="mb-1 block text-xs font-medium text-body/70">Full Name</label>
          <input type="text" placeholder="Your name" className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
        </div>
        <div>
          <label className="mb-1 block text-xs font-medium text-body/70">Email</label>
          <input type="email" placeholder="you@example.com" className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
        </div>
        <div>
          <label className="mb-1 block text-xs font-medium text-body/70">Password</label>
          <input type="password" placeholder="••••••••" className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm" />
        </div>
        <div>
          <label className="mb-1 block text-xs font-medium text-body/70">I am joining as</label>
          <select className="w-full rounded-md border border-border bg-background px-3 py-2 text-sm">
            {roles.map((role) => (
              <option key={role}>{role}</option>
            ))}
          </select>
        </div>
        <Button type="submit" variant="primary" className="w-full">
          Create Account
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-body/60">
        Already have an account?{' '}
        <Link to={routePaths.login} className="font-medium text-link hover:underline">
          Log in
        </Link>
      </p>
    </div>
  );
}
