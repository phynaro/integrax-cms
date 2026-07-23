import { apiClient, ApiResponse } from '@/lib/api-client';
import { Client, CreateClientRequest, UpdateClientRequest } from './types';

export const clientsApi = {
  getAll: async (params?: { page?: number; pageSize?: number; search?: string }) => {
    const { data } = await apiClient.get<ApiResponse<Client[]>>('/clients', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<Client>>(`/clients/${id}`);
    return data.data;
  },

  create: async (request: CreateClientRequest) => {
    const { data } = await apiClient.post<ApiResponse<Client>>('/clients', request);
    return data.data;
  },

  update: async ({ id, ...request }: UpdateClientRequest) => {
    const { data } = await apiClient.put<ApiResponse<Client>>(`/clients/${id}`, request);
    return data.data;
  },

  delete: async (id: string) => {
    await apiClient.delete(`/clients/${id}`);
  },
};
