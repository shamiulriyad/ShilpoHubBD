import { useState } from 'react';
import { Link, useLocation, useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { authService } from '../../services/authService';
import { useAuthStore } from '../../stores/useAuthStore';
import { resolveActiveRole, roleHomePath } from '../../utils/roles';
import { getApiErrorMessage } from '../../utils/apiError';

const isAuthRoute = (path) =>
  ['/login', '/register', '/forgot-password', '/reset-password', '/unauthorized'].some((p) =>
    path.startsWith(p),
  );

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [searchParams] = useSearchParams();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');

  const mutation = useMutation({
    mutationFn: authService.login,
    onSuccess: (data) => {
      useAuthStore.getState().setSession(data);
      const roleHome = roleHomePath(resolveActiveRole(data.roles ?? [], data.activeRole));
      const from = location.state?.from?.pathname;
      const target = from && !isAuthRoute(from) ? from : roleHome;
      navigate(target, { replace: true });
    },
  });

  const handleSubmit = (event) => {
    event.preventDefault();
    if (mutation.isPending) return;
    mutation.mutate({ email: email.trim(), password });
  };

  const errorMessage = mutation.error ? getApiErrorMessage(mutation.error, 'Unable to log in.') : null;
  const sessionExpired = searchParams.get('reason') === 'session-expired';
  const passwordReset = searchParams.get('reset') === 'success';

  return (
    <div>
      <h1 className="text-2xl font-semibold text-heading">Welcome back</h1>
      <p className="mt-1 text-base text-body/60">Log in to your ShilpoHub account.</p>

      {sessionExpired && !errorMessage && (
        <p role="status" className="mt-4 rounded-md border border-border bg-background px-3 py-2.5 text-sm text-body/70">
          Your previous session expired. Please sign in again.
        </p>
      )}

      {passwordReset && !errorMessage && (
        <p role="status" className="mt-4 rounded-md border border-success/30 bg-success/10 px-3 py-2.5 text-sm text-success">
          Your password was reset successfully. You can sign in with your new password.
        </p>
      )}

      {errorMessage && (
        <p role="alert" className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-700">
          {errorMessage}
        </p>
      )}

      <form className="mt-6 space-y-5" onSubmit={handleSubmit} noValidate>
        <div>
          <label htmlFor="login-email" className="mb-1.5 block text-sm font-medium text-body/70">Email</label>
          <input aria-label="You@example.com"
            id="login-email"
            type="email"
            required
            autoComplete="email"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
            placeholder="you@example.com"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
          />
        </div>
        <div>
          <label htmlFor="login-password" className="mb-1.5 block text-sm font-medium text-body/70">Password</label>
          <input
            id="login-password"
            type="password"
            required
            autoComplete="current-password"
            value={password}
            onChange={(event) => setPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
          />
        </div>
        <div className="flex justify-end text-sm">
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