import { apiClient, ApiResponse } from '@/lib/api-client';
import { 
  CollectionCase, 
  CaseListItem, 
  UpdateCaseRequest,
  AssignCaseRequest,
  CaseStatus,
  CasePriority
} from './types';

export const casesApi = {
  getAll: async (params?: { 
    page?: number; 
    pageSize?: number; 
    assignedToId?: string;
    status?: CaseStatus;
    priority?: CasePriority;
    search?: string;
  }) => {
    const { data } = await apiClient.get<ApiResponse<CaseListItem[]>>('/cases', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<CollectionCase>>(`/cases/${id}`);
    return data.data;
  },

  getByCaseNumber: async (caseNumber: string) => {
    const { data } = await apiClient.get<ApiResponse<CollectionCase>>(`/cases/by-case-number/${caseNumber}`);
    return data.data;
  },

  getMyCases: async () => {
    const { data } = await apiClient.get<ApiResponse<CaseListItem[]>>('/cases/my-cases');
    return data.data;
  },

  update: async ({ id, ...request }: UpdateCaseRequest) => {
    const { data } = await apiClient.put<ApiResponse<CollectionCase>>(`/cases/${id}`, request);
    return data.data;
  },

  assign: async (id: string, request: AssignCaseRequest) => {
    const { data } = await apiClient.post<ApiResponse<CollectionCase>>(`/cases/${id}/assign`, request);
    return data.data;
  },

  unassign: async (id: string) => {
    const { data } = await apiClient.post<ApiResponse<CollectionCase>>(`/cases/${id}/unassign`);
    return data.data;
  },

  close: async (id: string) => {
    const { data } = await apiClient.post<ApiResponse<CollectionCase>>(`/cases/${id}/close`);
    return data.data;
  },

  reopen: async (id: string) => {
    const { data } = await apiClient.post<ApiResponse<CollectionCase>>(`/cases/${id}/reopen`);
    return data.data;
  },
};
