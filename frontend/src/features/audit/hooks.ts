import { useQuery } from '@tanstack/react-query';
import { auditApi } from './api';
import { AuditFilters } from './types';

export const useAuditEvents = (params?: AuditFilters) => {
  return useQuery({
    queryKey: ['audit', params],
    queryFn: () => auditApi.getAll(params),
  });
};

export const useEntityHistory = (entityType: string, entityId: string) => {
  return useQuery({
    queryKey: ['audit', 'entity', entityType, entityId],
    queryFn: () => auditApi.getEntityHistory(entityType, entityId),
    enabled: !!entityType && !!entityId,
  });
};

export const useUserActivity = (userId: string) => {
  return useQuery({
    queryKey: ['audit', 'user', userId],
    queryFn: () => auditApi.getUserActivity(userId),
    enabled: !!userId,
  });
};
