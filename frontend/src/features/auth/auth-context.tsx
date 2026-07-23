import { createContext, useContext, useEffect, useState, ReactNode, useCallback } from 'react';
import { apiClient } from '@/lib/api-client';
import { AuthState, CurrentUser } from './types';

interface AuthContextValue extends AuthState {
  login: () => Promise<void>;
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

const PKCE_VERIFIER_KEY = 'pkce_code_verifier';

function generateCodeVerifier(): string {
  const array = new Uint8Array(32);
  crypto.getRandomValues(array);
  return base64UrlEncode(array);
}

async function generateCodeChallenge(verifier: string): Promise<string> {
  const encoder = new TextEncoder();
  const data = encoder.encode(verifier);
  const digest = await crypto.subtle.digest('SHA-256', data);
  return base64UrlEncode(new Uint8Array(digest));
}

function base64UrlEncode(array: Uint8Array): string {
  const base64 = btoa(String.fromCharCode(...array));
  return base64.replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
}

const getAuthUrl = async (): Promise<string> => {
  const redirectUri = encodeURIComponent(window.location.origin + '/auth/callback');
  const codeVerifier = generateCodeVerifier();
  const codeChallenge = await generateCodeChallenge(codeVerifier);
  
  sessionStorage.setItem(PKCE_VERIFIER_KEY, codeVerifier);
  
  return `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/auth?client_id=${KEYCLOAK_CLIENT_ID}&redirect_uri=${redirectUri}&response_type=code&scope=openid%20profile%20email&code_challenge=${codeChallenge}&code_challenge_method=S256`;
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
    token: localStorage.getItem('access_token'),
  });

  const fetchCurrentUser = useCallback(async (): Promise<CurrentUser | null> => {
    try {
      const response = await apiClient.get<{ data: CurrentUser }>('/auth/me');
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

  const login = async () => {
    const authUrl = await getAuthUrl();
    window.location.href = authUrl;
  };

  const logout = () => {
    localStorage.removeItem('access_token');
    setState({
      isAuthenticated: false,
      isLoading: false,
      user: null,
      token: null,
    });
    window.location.href = getLogoutUrl();
  };

  const setToken = (token: string) => {
    localStorage.setItem('access_token', token);
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
