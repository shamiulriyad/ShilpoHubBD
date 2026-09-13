import { useState } from 'react';
import { Link, useNavigate, useSearchParams } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { authService } from '../../services/authService';
import { getApiErrorMessage } from '../../utils/apiError';
import { validatePassword } from '../../utils/validation';

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const email = searchParams.get('email') || '';
  const token = searchParams.get('token') || '';
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [localError, setLocalError] = useState('');

  const mutation = useMutation({
    mutationFn: () => authService.resetPassword({ email, token, newPassword, confirmPassword }),
    onSuccess: () => navigate(`${routePaths.login}?reset=success`, { replace: true }),
  });

  const handleSubmit = (event) => {
    event.preventDefault();
    const passwordError = validatePassword(newPassword);
    if (passwordError) {
      setLocalError(passwordError);
      return;
    }
    if (newPassword !== confirmPassword) {
      setLocalError('Passwords do not match.');
      return;
    }
    setLocalError('');
    if (!mutation.isPending) mutation.mutate();
  };

  const errorMessage = localError || (mutation.error ? getApiErrorMessage(mutation.error, 'Could not reset your password.') : '');

  if (!email || !token) {
    return (
      <div>
        <h1 className="text-2xl font-semibold text-heading">Invalid reset link</h1>
        <p className="mt-1 text-base text-body/60">
          This link is missing required information. Request a new one from the{' '}
          <Link to={routePaths.forgotPassword} className="text-link hover:underline">forgot password</Link> page.
        </p>
      </div>
    );
  }

  return (
    <div>
      <h1 className="text-2xl font-semibold text-heading">Reset your password</h1>
      <p className="mt-1 break-words text-base text-body/60">Set a new password for {email}.</p>

      {errorMessage && (
        <p role="alert" className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-700">{errorMessage}</p>
      )}

      <form className="mt-6 space-y-5" onSubmit={handleSubmit} noValidate>
        <div>
          <label htmlFor="reset-password" className="mb-1.5 block text-sm font-medium text-body/70">New password</label>
          <input
            id="reset-password"
            type="password"
            required
            minLength={8}
            autoComplete="new-password"
            value={newPassword}
            onChange={(event) => setNewPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
          />
          <p className="mt-1 text-xs text-body/50">At least 8 characters with uppercase, lowercase, and a number.</p>
        </div>
        <div>
          <label htmlFor="reset-confirm" className="mb-1.5 block text-sm font-medium text-body/70">Confirm new password</label>
          <input
            id="reset-confirm"
            type="password"
            required
            minLength={8}
            autoComplete="new-password"
            value={confirmPassword}
            onChange={(event) => setConfirmPassword(event.target.value)}
            placeholder="••••••••"
            className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
          />
        </div>
        <Button type="submit" variant="primary" size="lg" className="w-full" disabled={mutation.isPending}>
          {mutation.isPending ? 'Resetting…' : 'Reset password'}
        </Button>
      </form>
    </div>
  );
}