import { apiClient, ApiResponse } from '@/lib/api-client';
import { User, Role, CreateUserRequest, UpdateUserRequest, ChangeRoleRequest } from './types';

export const usersApi = {
  getAll: async (params?: { page?: number; pageSize?: number; search?: string }) => {
    const { data } = await apiClient.get<ApiResponse<User[]>>('/users', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<User>>(`/users/${id}`);
    return data.data;
  },

  create: async (request: CreateUserRequest) => {
    const { data } = await apiClient.post<ApiResponse<User>>('/users', request);
    return data.data;
  },

  update: async ({ id, ...request }: UpdateUserRequest) => {
    const { data } = await apiClient.put<ApiResponse<User>>(`/users/${id}`, request);
    return data.data;
  },

  delete: async (id: string) => {
    await apiClient.delete(`/users/${id}`);
  },

  changeRole: async ({ userId, roleId }: ChangeRoleRequest) => {
    const { data } = await apiClient.put<ApiResponse<User>>(`/users/${userId}/role`, { roleId });
    return data.data;
  },

  activate: async (id: string) => {
    const { data } = await apiClient.put<ApiResponse<User>>(`/users/${id}/activate`);
    return data.data;
  },

  deactivate: async (id: string) => {
    const { data } = await apiClient.put<ApiResponse<User>>(`/users/${id}/deactivate`);
    return data.data;
  },
};

export const rolesApi = {
  getAll: async () => {
    const { data } = await apiClient.get<ApiResponse<Role[]>>('/roles');
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<Role>>(`/roles/${id}`);
    return data.data;
  },
};
