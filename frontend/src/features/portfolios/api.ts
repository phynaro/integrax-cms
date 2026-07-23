import { apiClient, ApiResponse } from '@/lib/api-client';
import { Portfolio, CreatePortfolioRequest, UpdatePortfolioRequest, ChangeStatusRequest, PortfolioStatus } from './types';

export const portfoliosApi = {
  getAll: async (params?: { 
    page?: number; 
    pageSize?: number; 
    search?: string;
    clientId?: string;
    status?: PortfolioStatus;
  }) => {
    const { data } = await apiClient.get<ApiResponse<Portfolio[]>>('/portfolios', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<Portfolio>>(`/portfolios/${id}`);
    return data.data;
  },

  create: async (request: CreatePortfolioRequest) => {
    const { data } = await apiClient.post<ApiResponse<Portfolio>>('/portfolios', request);
    return data.data;
  },

  update: async ({ id, ...request }: UpdatePortfolioRequest) => {
    const { data } = await apiClient.put<ApiResponse<Portfolio>>(`/portfolios/${id}`, request);
    return data.data;
  },

  delete: async (id: string) => {
    await apiClient.delete(`/portfolios/${id}`);
  },

  changeStatus: async ({ portfolioId, status }: ChangeStatusRequest) => {
    const { data } = await apiClient.put<ApiResponse<Portfolio>>(`/portfolios/${portfolioId}/status`, { status });
    return data.data;
  },

  getByClientId: async (clientId: string) => {
    const { data } = await apiClient.get<ApiResponse<Portfolio[]>>(`/clients/${clientId}/portfolios`);
    return data;
  },
};
