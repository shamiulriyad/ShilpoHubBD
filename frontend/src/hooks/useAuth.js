import { useAuthStore } from '../stores/useAuthStore';
import { authService } from '../services/authService';

export function useAuth() {
  const { accessToken, user, roles, activeRole, refreshToken, hasRole, clearSession, setSession } =
    useAuthStore();

  const currentRole = activeRole ?? roles[0] ?? null;

  return {
    user: user ? { ...user, name: user.fullName, role: currentRole } : null,
    roles,
    activeRole: currentRole,
    isAuthenticated: Boolean(accessToken),
    hasRole,
    switchRole: async (role) => {
      const data = await authService.switchRole(role);
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
