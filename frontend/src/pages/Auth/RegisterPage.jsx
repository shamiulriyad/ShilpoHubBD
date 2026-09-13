import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useMutation } from '@tanstack/react-query';
import { routePaths } from '../../routes/routePaths';
import { Button } from '../../components/ui';
import { authService } from '../../services/authService';
import { useAuthStore } from '../../stores/useAuthStore';
import { resolveActiveRole, roleHomePath } from '../../utils/roles';
import { getApiErrorMessage } from '../../utils/apiError';
import { validatePassword } from '../../utils/validation';
import { ACCOUNT_TYPES } from './accountTypes';

export default function RegisterPage() {
  const navigate = useNavigate();
  const [step, setStep] = useState(1);
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [selectedRole, setSelectedRole] = useState(null);
  const [step1Error, setStep1Error] = useState('');

  const mutation = useMutation({
    mutationFn: authService.register,
    onSuccess: (data) => {
      useAuthStore.getState().setSession(data);
      navigate(roleHomePath(resolveActiveRole(data.roles ?? [], data.activeRole)), { replace: true });
    },
    onError: (error) => {
      const errors = error?.response?.data?.errors;
      const touchesStep1Field =
        errors && Object.keys(errors).some((key) => key !== 'Roles' && !key.startsWith('Roles'));
      if (touchesStep1Field) setStep(1);
    },
  });

  const handleContinue = (event) => {
    event.preventDefault();
    const normalizedName = fullName.trim();
    const normalizedEmail = email.trim();

    if (!normalizedName) {
      setStep1Error('Enter your full name.');
      return;
    }
    if (!/^\S+@\S+\.\S+$/.test(normalizedEmail)) {
      setStep1Error('Enter a valid email address.');
      return;
    }

    const passwordError = validatePassword(password);
    if (passwordError) {
      setStep1Error(passwordError);
      return;
    }
    if (password !== confirmPassword) {
      setStep1Error('Passwords do not match.');
      return;
    }

    setStep1Error('');
    setStep(2);
  };

  const handleSubmit = (event) => {
    event.preventDefault();
    if (!selectedRole || mutation.isPending) return;
    mutation.mutate({
      fullName: fullName.trim(),
      email: email.trim(),
      password,
      confirmPassword,
      roles: [selectedRole],
    });
  };

  const fieldErrors = mutation.error?.response?.data?.errors;
  const generalError = mutation.error && !fieldErrors
    ? getApiErrorMessage(mutation.error, 'Unable to create your account.')
    : null;

  return (
    <div>
      <div className="mb-6 flex items-center gap-2 text-sm font-medium text-body/50" aria-label={`Registration step ${step} of 2`}>
        <StepDot active={step >= 1} done={step > 1} label="1" />
        <span className="h-px w-8 bg-border" />
        <StepDot active={step >= 2} done={false} label="2" />
        <span className="ml-2">Step {step} of 2</span>
      </div>

      {step === 1 ? (
        <>
          <h1 className="text-2xl font-semibold text-heading">Create your account</h1>
          <p className="mt-1 text-base text-body/60">Join the ShilpoHub heritage ecosystem.</p>

          {step1Error && (
            <p role="alert" className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-700">
              {step1Error}
            </p>
          )}

          <form className="mx-auto mt-6 max-w-md space-y-5" onSubmit={handleContinue} noValidate>
            <div>
              <label htmlFor="register-name" className="mb-1.5 block text-sm font-medium text-body/70">Full Name</label>
              <input aria-label="Your name"
                id="register-name"
                type="text"
                required
                maxLength={200}
                autoComplete="name"
                value={fullName}
                onChange={(event) => setFullName(event.target.value)}
                placeholder="Your name"
                className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
              {fieldErrors?.FullName && <p className="mt-1 text-sm text-red-700">{fieldErrors.FullName[0]}</p>}
            </div>
            <div>
              <label htmlFor="register-email" className="mb-1.5 block text-sm font-medium text-body/70">Email</label>
              <input aria-label="You@example.com"
                id="register-email"
                type="email"
                required
                autoComplete="email"
                value={email}
                onChange={(event) => setEmail(event.target.value)}
                placeholder="you@example.com"
                className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
              {fieldErrors?.Email && <p className="mt-1 text-sm text-red-700">{fieldErrors.Email[0]}</p>}
            </div>
            <div>
              <label htmlFor="register-password" className="mb-1.5 block text-sm font-medium text-body/70">Password</label>
              <input
                id="register-password"
                type="password"
                required
                minLength={8}
                autoComplete="new-password"
                value={password}
                onChange={(event) => setPassword(event.target.value)}
                placeholder="••••••••"
                className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
              <p className="mt-1 text-xs text-body/50">At least 8 characters with uppercase, lowercase, and a number.</p>
              {fieldErrors?.Password && <p className="mt-1 text-sm text-red-700">{fieldErrors.Password[0]}</p>}
            </div>
            <div>
              <label htmlFor="register-confirm" className="mb-1.5 block text-sm font-medium text-body/70">Confirm Password</label>
              <input
                id="register-confirm"
                type="password"
                required
                minLength={8}
                autoComplete="new-password"
                value={confirmPassword}
                onChange={(event) => setConfirmPassword(event.target.value)}
                placeholder="••••••••"
                className="w-full rounded-md border border-border bg-background px-3.5 py-2.5 text-base focus:border-primary focus:outline-none focus:ring-4 focus:ring-primary/10"
              />
              {fieldErrors?.ConfirmPassword && (
                <p className="mt-1 text-sm text-red-700">{fieldErrors.ConfirmPassword[0]}</p>
              )}
            </div>
            <Button type="submit" variant="primary" size="lg" className="w-full">Continue</Button>
          </form>

          <p className="mt-6 text-center text-base text-body/60">
            Already have an account?{' '}
            <Link to={routePaths.login} className="font-medium text-link hover:underline">Log in</Link>
          </p>
        </>
      ) : (
        <>
          <h1 className="text-2xl font-semibold text-heading">Choose your account type</h1>
          <p className="mt-1 text-base text-body/60">Choose the workspace that best matches how you will use ShilpoHub.</p>

          {generalError && (
            <p role="alert" className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-700">
              {generalError}
            </p>
          )}
          {fieldErrors && (
            <p role="alert" className="mt-4 rounded-md border border-red-200 bg-red-50 px-3 py-2.5 text-sm text-red-700">
              {Object.values(fieldErrors).flat()[0]}
            </p>
          )}

          <form className="mt-6" onSubmit={handleSubmit}>
            <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4" role="radiogroup" aria-label="Account type">
              {ACCOUNT_TYPES.map(({ id, label, description, Icon }) => {
                const isSelected = selectedRole === id;
                return (
                  <button
                    key={id}
                    type="button"
                    role="radio"
                    aria-checked={isSelected}
                    onClick={() => setSelectedRole(id)}
                    className={`relative flex flex-col items-start gap-2.5 rounded-xl border p-5 text-left transition focus:outline-none focus:ring-4 focus:ring-primary/15 ${
                      isSelected
                        ? 'border-primary bg-primary/5 ring-2 ring-primary'
                        : 'border-border bg-background hover:border-primary/40 hover:bg-primary/5'
                    }`}
                  >
                    <span className={`flex h-11 w-11 items-center justify-center rounded-lg ${isSelected ? 'bg-primary text-surface' : 'bg-primary/10 text-primary'}`}>
                      <Icon />
                    </span>
                    <span className="text-base font-semibold text-heading">{label}</span>
                    <span className="text-sm leading-snug text-body/60">{description}</span>
                  </button>
                );
              })}
            </div>

            <div className="mt-6 flex flex-col-reverse gap-3 sm:flex-row sm:items-center">
              <Button type="button" variant="secondary" size="lg" onClick={() => setStep(1)} className="sm:w-auto">Back</Button>
              <Button type="submit" variant="primary" size="lg" className="flex-1" disabled={!selectedRole || mutation.isPending}>
                {mutation.isPending ? 'Creating account…' : 'Create Account'}
              </Button>
            </div>
          </form>
        </>
      )}
    </div>
  );
}

function StepDot({ active, done, label }) {
  return (
    <span className={`flex h-6 w-6 items-center justify-center rounded-full text-xs font-semibold ${
      done ? 'bg-primary text-surface' : active ? 'border-2 border-primary text-primary' : 'border border-border text-body/40'
    }`}>
      {done ? '✓' : label}
    </span>
  );
}
