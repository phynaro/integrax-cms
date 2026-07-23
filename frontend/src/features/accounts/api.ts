import { apiClient, ApiResponse } from '@/lib/api-client';
import { 
  DebtAccount, 
  DebtAccountListItem, 
  CreateDebtAccountRequest, 
  UpdateDebtAccountRequest,
  RecordPaymentRequest,
  AccountStatus
} from './types';

export const accountsApi = {
  getAll: async (params?: { 
    page?: number; 
    pageSize?: number; 
    portfolioId?: string;
    debtorId?: string;
    status?: AccountStatus;
    search?: string;
  }) => {
    const { data } = await apiClient.get<ApiResponse<DebtAccountListItem[]>>('/debtaccounts', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<DebtAccount>>(`/debtaccounts/${id}`);
    return data.data;
  },

  getByAccountNumber: async (accountNumber: string) => {
    const { data } = await apiClient.get<ApiResponse<DebtAccount>>(`/debtaccounts/by-account-number/${accountNumber}`);
    return data.data;
  },

  create: async (request: CreateDebtAccountRequest) => {
    const { data } = await apiClient.post<ApiResponse<DebtAccount>>('/debtaccounts', request);
    return data.data;
  },

  update: async ({ id, ...request }: UpdateDebtAccountRequest) => {
    const { data } = await apiClient.put<ApiResponse<DebtAccount>>(`/debtaccounts/${id}`, request);
    return data.data;
  },

  delete: async (id: string) => {
    await apiClient.delete(`/debtaccounts/${id}`);
  },

  recordPayment: async (id: string, request: RecordPaymentRequest) => {
    const { data } = await apiClient.post<ApiResponse<DebtAccount>>(`/debtaccounts/${id}/record-payment`, request);
    return data.data;
  },
};
