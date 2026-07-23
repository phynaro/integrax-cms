import { apiClient, ApiResponse } from '@/lib/api-client';
import { 
  Debtor, 
  DebtorListItem, 
  CreateDebtorRequest, 
  UpdateDebtorRequest,
  DebtorContact,
  DebtorAddress,
  CreateContactRequest,
  CreateAddressRequest
} from './types';

export const debtorsApi = {
  getAll: async (params?: { page?: number; pageSize?: number; search?: string }) => {
    const { data } = await apiClient.get<ApiResponse<DebtorListItem[]>>('/debtors', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<Debtor>>(`/debtors/${id}`);
    return data.data;
  },

  getByExternalId: async (externalId: string) => {
    const { data } = await apiClient.get<ApiResponse<Debtor>>(`/debtors/by-external/${externalId}`);
    return data.data;
  },

  create: async (request: CreateDebtorRequest) => {
    const { data } = await apiClient.post<ApiResponse<Debtor>>('/debtors', request);
    return data.data;
  },

  update: async ({ id, ...request }: UpdateDebtorRequest) => {
    const { data } = await apiClient.put<ApiResponse<Debtor>>(`/debtors/${id}`, request);
    return data.data;
  },

  delete: async (id: string) => {
    await apiClient.delete(`/debtors/${id}`);
  },

  addContact: async (debtorId: string, request: CreateContactRequest) => {
    const { data } = await apiClient.post<ApiResponse<DebtorContact>>(`/debtors/${debtorId}/contacts`, request);
    return data.data;
  },

  updateContact: async (debtorId: string, contactId: string, request: CreateContactRequest) => {
    const { data } = await apiClient.put<ApiResponse<DebtorContact>>(`/debtors/${debtorId}/contacts/${contactId}`, request);
    return data.data;
  },

  deleteContact: async (debtorId: string, contactId: string) => {
    await apiClient.delete(`/debtors/${debtorId}/contacts/${contactId}`);
  },

  addAddress: async (debtorId: string, request: CreateAddressRequest) => {
    const { data } = await apiClient.post<ApiResponse<DebtorAddress>>(`/debtors/${debtorId}/addresses`, request);
    return data.data;
  },

  updateAddress: async (debtorId: string, addressId: string, request: CreateAddressRequest) => {
    const { data } = await apiClient.put<ApiResponse<DebtorAddress>>(`/debtors/${debtorId}/addresses/${addressId}`, request);
    return data.data;
  },

  deleteAddress: async (debtorId: string, addressId: string) => {
    await apiClient.delete(`/debtors/${debtorId}/addresses/${addressId}`);
  },
};
