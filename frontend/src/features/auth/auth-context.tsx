import { createContext, useContext, useEffect, useState, ReactNode, useCallback } from 'react';
import { apiClient } from '@/lib/api-client';
import { AuthState, CurrentUser } from './types';

interface AuthContextValue extends AuthState {
  login: () => void;
  logout: () => void;
  setToken: (token: string) => void;
  refreshUser: () => Promise<void>;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined);

const KEYCLOAK_URL = import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8080';
const KEYCLOAK_REALM = import.meta.env.VITE_KEYCLOAK_REALM || 'debtcollection';
const KEYCLOAK_CLIENT_ID = import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'debt-collection-web';

const getAuthUrl = () => {
  const redirectUri = encodeURIComponent(window.location.origin + '/auth/callback');
  return `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/auth?client_id=${KEYCLOAK_CLIENT_ID}&redirect_uri=${redirectUri}&response_type=code&scope=openid%20profile%20email`;
};

const getLogoutUrl = () => {
  const redirectUri = encodeURIComponent(window.location.origin);
  return `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/logout?post_logout_redirect_uri=${redirectUri}`;
};

interface Props {
  children: ReactNode;
}

export function AuthProvider({ children }: Props) {
  const [state, setState] = useState<AuthState>({
    isAuthenticated: false,
    isLoading: true,
    user: null,
    token: localStorage.getItem('auth_token'),
  });

  const fetchCurrentUser = useCallback(async (): Promise<CurrentUser | null> => {
    try {
      const response = await apiClient.get<{ data: CurrentUser }>('/api/v1/auth/me');
      return response.data.data;
    } catch {
      return null;
    }
  }, []);

  const refreshUser = useCallback(async () => {
    if (!state.token) {
      setState(prev => ({ ...prev, isLoading: false }));
      return;
    }

    setState(prev => ({ ...prev, isLoading: true }));
    const user = await fetchCurrentUser();
    
    setState(prev => ({
      ...prev,
      isLoading: false,
      isAuthenticated: user !== null,
      user,
    }));
  }, [state.token, fetchCurrentUser]);

  useEffect(() => {
    refreshUser();
  }, []);

  const login = () => {
    window.location.href = getAuthUrl();
  };

  const logout = () => {
    localStorage.removeItem('auth_token');
    setState({
      isAuthenticated: false,
      isLoading: false,
      user: null,
      token: null,
    });
    window.location.href = getLogoutUrl();
  };

  const setToken = (token: string) => {
    localStorage.setItem('auth_token', token);
    setState(prev => ({ ...prev, token }));
    refreshUser();
  };

  const hasPermission = (permission: string): boolean => {
    if (!state.user) return false;
    const perms = state.user.permissions;
    return perms.includes('*') || perms.includes(permission);
  };

  const hasRole = (role: string): boolean => {
    return state.user?.role === role;
  };

  return (
    <AuthContext.Provider
      value={{
        ...state,
        login,
        logout,
        setToken,
        refreshUser,
        hasPermission,
        hasRole,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
