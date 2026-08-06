import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { authService } from '../../services/authService';

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email') || '';
  const token = searchParams.get('token') || '';

  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');

  const mutation = useMutation({
    mutationFn: () => authService.resetPassword({ email, token, newPassword, confirmPassword }),
    onSuccess: () => navigate(routePaths.login, { replace: true }),
  });

  const handleSubmit = (event) => {
    event.preventDefault();
    mutation.mutate();
  };

  const errorMessage = mutation.error?.response?.data?.title || mutation.error?.message;

  if (!email || !token) {
    return (
      <div>
        <h1 className="text-2xl font-semibold text-heading">Invalid reset link</h1>
        <p className="mt-1 text-base text-body/60">
          This link is missing required information. Request a new one from the{' '}
          <Link to={routePaths.forgotPassword} className="text-link hover:underline">
            forgot password
          </Link>{' '}
          page.
        </p>
      </div>
    );
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold text-heading">Reset your password</h1>
      <p className="mt-1 text-base text-body/60">Set a new password for {email}.</p>

      {errorMessage && (
        <p className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-base text-red-600">
          {errorMessage}
        </p>
      )}

      <form className="mt-6 space-y-5" onSubmit={handleSubmit}>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-body/70">New password</label>
          <input
            type="password"
            required
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base"
          />
        </div>
        <div>
          <label className="mb-1.5 block text-sm font-medium text-body/70">Confirm new password</label>
          <input
            type="password"
            required
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base"
          />
        </div>
        <Button type="submit" variant="primary" size="lg" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Resetting…' : 'Reset password'}
        </Button>
      </form>
    </div>
  );
}
