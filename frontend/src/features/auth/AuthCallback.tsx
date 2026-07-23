import { useEffect, useRef, useState } from 'react';
import { useAuth } from './auth-context';
import { apiClient } from '@/lib/api-client';

const KEYCLOAK_URL = import.meta.env.VITE_KEYCLOAK_URL || 'http://localhost:8080';
const KEYCLOAK_REALM = import.meta.env.VITE_KEYCLOAK_REALM || 'debtcollection';
const KEYCLOAK_CLIENT_ID = import.meta.env.VITE_KEYCLOAK_CLIENT_ID || 'debt-collection-web';
const PKCE_VERIFIER_KEY = 'pkce_code_verifier';

export function AuthCallback() {
  const { setToken } = useAuth();
  const [error, setError] = useState<string | null>(null);
  const exchangedRef = useRef(false);

  useEffect(() => {
    const handleCallback = async () => {
      if (exchangedRef.current) return;
      exchangedRef.current = true;

      const urlParams = new URLSearchParams(window.location.search);
      const code = urlParams.get('code');
      const errorParam = urlParams.get('error');
      const errorDescription = urlParams.get('error_description');

      if (errorParam) {
        setError(errorDescription || errorParam);
        return;
      }

      if (!code) {
        setError('No authorization code received');
        return;
      }

      const codeVerifier = sessionStorage.getItem(PKCE_VERIFIER_KEY);
      if (!codeVerifier) {
        setError('Missing PKCE code verifier. Please try logging in again.');
        return;
      }

      try {
        const tokenUrl = `${KEYCLOAK_URL}/realms/${KEYCLOAK_REALM}/protocol/openid-connect/token`;
        const redirectUri = window.location.origin + '/auth/callback';

        const response = await fetch(tokenUrl, {
          method: 'POST',
          headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
          },
          body: new URLSearchParams({
            grant_type: 'authorization_code',
            client_id: KEYCLOAK_CLIENT_ID,
            code,
            redirect_uri: redirectUri,
            code_verifier: codeVerifier,
          }),
        });

        sessionStorage.removeItem(PKCE_VERIFIER_KEY);

        if (!response.ok) {
          const errorData = await response.json().catch(() => ({}));
          throw new Error(errorData.error_description || 'Failed to exchange code for token');
        }

        const data = await response.json();
        localStorage.setItem('access_token', data.access_token);
        setToken(data.access_token);

        await apiClient.post('/auth/sync');

        window.location.replace('/');
      } catch (err) {
        sessionStorage.removeItem(PKCE_VERIFIER_KEY);
        setError(err instanceof Error ? err.message : 'Authentication failed');
      }
    };

    handleCallback();
  }, [setToken]);

  if (error) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-background">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-destructive mb-2">Authentication Error</h1>
          <p className="text-muted-foreground mb-4">{error}</p>
          <a href="/" className="text-primary hover:underline">
            Return to home
          </a>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="text-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4" />
        <p className="text-muted-foreground">Completing authentication...</p>
      </div>
    </div>
  );
}
