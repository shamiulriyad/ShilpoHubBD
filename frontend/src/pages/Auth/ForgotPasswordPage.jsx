import { useState } from 'react';
import { Link } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { authService } from '../../services/authService';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');

  const mutation = useMutation({
    mutationFn: () => authService.forgotPassword(email),
  });

  const handleSubmit = (event) => {
    event.preventDefault();
    mutation.mutate();
  };

  return (
    <div>
      <h1 className="text-2xl font-semibold text-heading">Forgot password</h1>
      <p className="mt-1 text-base text-body/60">
        Enter your account email and we'll send you a link to reset your password.
      </p>

      {mutation.isSuccess ? (
        <p className="mt-6 rounded-md border border-border bg-background px-3.5 py-2.5 text-base text-body/70">
          If that email is registered, a password reset link has been sent.
        </p>
      ) : (
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
          <Button type="submit" variant="primary" size="lg" className="w-full" disabled={mutation.isPending}>
            {mutation.isPending ? 'Sending…' : 'Send reset link'}
          </Button>
        </form>
      )}

      <p className="mt-6 text-center text-base text-body/60">
        Remembered your password?{' '}
        <Link to={routePaths.login} className="font-medium text-link hover:underline">
          Log in
        </Link>
      </p>
    </div>
  );
}
