import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { authService } from '../../services/authService';
import { useAuthStore } from '../../stores/useAuthStore';
import { roleHomePath } from '../../utils/roles';

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const mutation = useMutation({
    mutationFn: authService.login,
    onSuccess: (data) => {
      useAuthStore.getState().setSession(data);
      navigate(location.state?.from?.pathname || roleHomePath(data.activeRole), { replace: true });
    },
  });

  const handleSubmit = (event) => {
    event.preventDefault();
    mutation.mutate({ email, password });
  };

  const errorMessage = mutation.error?.response?.data?.title || mutation.error?.message;

  return (
    <div>
      <h1 className="text-2xl font-semibold text-heading">Welcome back</h1>
      <p className="mt-1 text-base text-body/60">Log in to your ShilpoHub account.</p>

      {errorMessage && (
        <p className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-base text-red-600">
          {errorMessage}
        </p>
      )}

      <form className="mt-6 space-y-5" onSubmit={handleSubmit}>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-body/70">Email</label>
          <input
            type="email"
            required
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@example.com"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base"
          />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-body/70">Password</label>
          <input
            type="password"
            required
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base"
          />
        </div>
        <div className="flex items-center justify-between text-sm">
          <label className="flex items-center gap-2 text-body/70">
            <input type="checkbox" className="h-4 w-4" />
            Remember me
          </label>
          <Link to={routePaths.forgotPassword} className="text-link hover:underline">
            Forgot password?
          </Link>
        </div>
        <Button type="submit" variant="primary" size="lg" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Logging in…' : 'Log In'}
        </Button>
      </form>

      <p className="mt-6 text-center text-base text-body/60">
        Don't have an account?{' '}
        <Link to={routePaths.register} className="font-medium text-link hover:underline">
          Register
        </Link>
      </p>
    </div>
  );
}
