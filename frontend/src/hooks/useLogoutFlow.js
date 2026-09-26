import { useCallback, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from './useAuth';

// One place for "sign out": the caller shows a ConfirmDialog while `confirming` is true, then
// `confirm()` revokes the session and lands on the public home page.
export function useLogoutFlow() {
  const navigate = useNavigate();
  const { logout } = useAuth();
  const [confirming, setConfirming] = useState(false);
  const [busy, setBusy] = useState(false);

  const request = useCallback(() => setConfirming(true), []);
  const cancel = useCallback(() => setConfirming(false), []);
  const confirm = useCallback(async () => {
    setBusy(true);
    try {
      await logout();
    } finally {
      setBusy(false);
      setConfirming(false);
      navigate('/', { replace: true });
    }
  }, [logout, navigate]);

  return { confirming, busy, request, cancel, confirm };
}
