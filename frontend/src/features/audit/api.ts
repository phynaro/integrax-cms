import { apiClient, ApiResponse } from '@/lib/api-client';
import { AuditEvent, AuditFilters } from './types';

export const auditApi = {
  getAll: async (params?: AuditFilters) => {
    const { data } = await apiClient.get<ApiResponse<AuditEvent[]>>('/audit', { params });
    return data;
  },

  getEntityHistory: async (entityType: string, entityId: string) => {
    const { data } = await apiClient.get<ApiResponse<AuditEvent[]>>(`/audit/entity/${entityType}/${entityId}`);
    return data;
  },

  getUserActivity: async (userId: string) => {
    const { data } = await apiClient.get<ApiResponse<AuditEvent[]>>(`/audit/user/${userId}`);
    return data;
  },
};
