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

      // /auth/logout is [Authorize]: it must go out while the access token is still in the store,
      // otherwise it 401s and the refresh token is never revoked. Bounded so a slow API cannot trap the user.
      if (tokenToRevoke) {
        try {
          await Promise.race([
            authService.logout(tokenToRevoke),
            new Promise((resolve) => setTimeout(resolve, 4000)),
          ]);
        } catch {
          // Local logout is authoritative for the client; server-side revocation is best effort.
        }
      }

      clearSession();
      queryClient.clear();
    },
  };
}
