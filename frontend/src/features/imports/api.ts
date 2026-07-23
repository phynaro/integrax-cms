import { apiClient, ApiResponse } from '@/lib/api-client';
import { 
  ImportBatch,
  ImportBatchListItem,
  ImportValidationResult,
  ConfirmImportRequest,
  ImportResult,
  RollbackResult
} from './types';

export const importsApi = {
  getAll: async (params?: { page?: number; pageSize?: number; portfolioId?: string }) => {
    const { data } = await apiClient.get<ApiResponse<ImportBatchListItem[]>>('/imports', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<ImportBatch>>(`/imports/${id}`);
    return data.data;
  },

  getTemplate: async () => {
    const response = await apiClient.get('/imports/template', { responseType: 'blob' });
    return response.data;
  },

  validate: async (portfolioId: string, file: File) => {
    const formData = new FormData();
    formData.append('file', file);
    const { data } = await apiClient.post<ApiResponse<ImportValidationResult>>(
      `/imports/validate?portfolioId=${portfolioId}`,
      formData,
      { headers: { 'Content-Type': 'multipart/form-data' } }
    );
    return data.data;
  },

  confirm: async (request: ConfirmImportRequest) => {
    const { data } = await apiClient.post<ApiResponse<ImportResult>>('/imports/confirm', request);
    return data.data;
  },

  rollback: async (id: string) => {
    const { data } = await apiClient.post<ApiResponse<RollbackResult>>(`/imports/${id}/rollback`);
    return data.data;
  },
};
