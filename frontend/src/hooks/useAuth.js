import { useAuthStore } from '../stores/useAuthStore';
import { authService } from '../services/authService';
import { queryClient } from '../lib/queryClient';
import { resolveActiveRole, roleHomePath, roleLabel } from '../utils/roles';

export function useAuth() {
  const accessToken = useAuthStore((s) => s.accessToken);
  const refreshToken = useAuthStore((s) => s.refreshToken);
  const user = useAuthStore((s) => s.user);
  const roles = useAuthStore((s) => s.roles);
  const storedActiveRole = useAuthStore((s) => s.activeRole);
  const sessionReady = useAuthStore((s) => s.sessionReady);
  const setSession = useAuthStore((s) => s.setSession);
  const clearSession = useAuthStore((s) => s.clearSession);

  const role = resolveActiveRole(roles ?? [], storedActiveRole);
  const isAuthenticated = Boolean(accessToken && user);

  return {
    user: user
      ? { ...user, name: user.fullName, role, roleLabel: role ? roleLabel(role) : null }
      : null,
    roles: roles ?? [],
    activeRole: role,
    isAuthenticated,
    isHydrated: sessionReady,

    hasRole: (r) => (roles ?? []).includes(r),
    hasAnyRole: (allowed = []) =>
      allowed.length === 0 || allowed.some((r) => (roles ?? []).includes(r)),

    homePath: role ? roleHomePath(role) : null,

    switchRole: async (nextRole) => {
      const data = await authService.switchRole(nextRole);
      queryClient.clear();
      setSession(data);
      return data;
    },
    logout: async () => {
      const tokenToRevoke = refreshToken;
      clearSession();
      queryClient.clear();

      if (!tokenToRevoke) return;

      try {
        await authService.logout(tokenToRevoke);
      } catch {
        // Local logout is authoritative for the client; server-side revocation is best effort.
      }
    },
  };
}
