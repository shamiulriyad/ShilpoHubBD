import { useAuthStore } from '../stores/useAuthStore';
import { authService } from '../services/authService';
import { resolveActiveRole, roleHomePath, roleLabel } from '../utils/roles';

/**
 * The single source of truth for the authenticated user and their role.
 * Every component reads identity from here — never from useAuthStore or
 * localStorage directly.
 */
export function useAuth() {
  const accessToken = useAuthStore((s) => s.accessToken);
  const refreshToken = useAuthStore((s) => s.refreshToken);
  const user = useAuthStore((s) => s.user);
  const roles = useAuthStore((s) => s.roles);
  const storedActiveRole = useAuthStore((s) => s.activeRole);
  const hasHydrated = useAuthStore((s) => s.hasHydrated);
  const setSession = useAuthStore((s) => s.setSession);
  const clearSession = useAuthStore((s) => s.clearSession);

  // `activeRole` is already normalised in the store; resolve again defensively
  // so a stale/legacy value can never leak a null role to the UI.
  const role = resolveActiveRole(roles ?? [], storedActiveRole);
  const isAuthenticated = Boolean(accessToken);

  return {
    // identity
    user: user
      ? { ...user, name: user.fullName, role, roleLabel: role ? roleLabel(role) : null }
      : null,
    roles: roles ?? [],
    activeRole: role,
    isAuthenticated,
    isHydrated: hasHydrated,

    // role checks
    hasRole: (r) => (roles ?? []).includes(r),
    hasAnyRole: (allowed = []) =>
      allowed.length === 0 || allowed.some((r) => (roles ?? []).includes(r)),

    // where this user belongs
    homePath: role ? roleHomePath(role) : null,

    // actions
    switchRole: async (nextRole) => {
      const data = await authService.switchRole(nextRole);
      setSession(data);
      return data;
    },
    logout: () => {
      if (refreshToken) {
        authService.logout(refreshToken).catch(() => {});
      }
      clearSession();
    },
  };
}
