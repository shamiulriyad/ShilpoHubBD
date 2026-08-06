import { useAuthStore } from '../stores/useAuthStore';
import { authService } from '../services/authService';

export function useAuth() {
  const { accessToken, user, roles, activeRole, refreshToken, hasRole, clearSession } = useAuthStore();

  return {
    user: user ? { ...user, name: user.fullName, role: activeRole ?? roles[0] ?? null } : null,
    roles,
    activeRole,
    isAuthenticated: Boolean(accessToken),
    hasRole,
    logout: () => {
      if (refreshToken) {
        authService.logout(refreshToken).catch(() => {});
      }
      clearSession();
    },
  };
}
