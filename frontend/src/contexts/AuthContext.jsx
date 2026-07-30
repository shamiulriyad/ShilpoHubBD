import { createContext, useContext, useMemo, useState } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
  const [user, setUser] = useState(() => {
    const saved = localStorage.getItem('user');
    return saved ? JSON.parse(saved) : null;
  });

  const value = useMemo(() => {
    const isAuthenticated = Boolean(user?.token);

    return {
      user,
      isAuthenticated,
      login: (nextUser) => {
        setUser(nextUser);
        localStorage.setItem('user', JSON.stringify(nextUser));
        localStorage.setItem('accessToken', nextUser.token);
      },
      logout: () => {
        setUser(null);
        localStorage.removeItem('user');
        localStorage.removeItem('accessToken');
      },
      hasRole: (role) => user?.role === role,
    };
  }, [user]);

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within AuthProvider');
  }

  return context;
}
