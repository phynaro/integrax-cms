export interface Role {
  id: string;
  name: string;
  description?: string;
  permissions: string[];
  isSystem: boolean;
}

export interface User {
  id: string;
  keycloakId: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName?: string;
  roleId: string;
  role: Role;
  isActive: boolean;
  lastLoginAt?: string;
  createdAt: string;
  createdBy?: { id: string; displayName: string };
  updatedAt?: string;
}

export interface CreateUserRequest {
  email: string;
  firstName: string;
  lastName: string;
  displayName?: string;
  roleId: string;
}

export interface UpdateUserRequest extends CreateUserRequest {
  id: string;
}

export interface ChangeRoleRequest {
  userId: string;
  roleId: string;
}
