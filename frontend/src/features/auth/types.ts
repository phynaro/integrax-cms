export interface CurrentUser {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  fullName: string;
  role: string;
  permissions: string[];
  isActive: boolean;
  lastLoginAt: string | null;
}

export interface AuthState {
  isAuthenticated: boolean;
  isLoading: boolean;
  user: CurrentUser | null;
  token: string | null;
}

export interface AuthCheckResponse {
  isAuthenticated: boolean;
  userId: string | null;
  email: string | null;
  role: string | null;
  permissions: string[];
}

export interface KeycloakConfig {
  url: string;
  realm: string;
  clientId: string;
}
