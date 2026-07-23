import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { casesApi } from './api';
import { UpdateCaseRequest, AssignCaseRequest, CaseStatus, CasePriority } from './types';

export const useCases = (params?: { 
  page?: number; 
  pageSize?: number; 
  assignedToId?: string;
  status?: CaseStatus;
  priority?: CasePriority;
  search?: string;
}) => {
  return useQuery({
    queryKey: ['cases', params],
    queryFn: () => casesApi.getAll(params),
  });
};

export const useCase = (id: string) => {
  return useQuery({
    queryKey: ['cases', id],
    queryFn: () => casesApi.getById(id),
    enabled: !!id,
  });
};

export const useMyCases = () => {
  return useQuery({
    queryKey: ['cases', 'my-cases'],
    queryFn: () => casesApi.getMyCases(),
  });
};

export const useUpdateCase = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateCaseRequest) => casesApi.update(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cases'] });
      queryClient.invalidateQueries({ queryKey: ['cases', variables.id] });
    },
  });
};

export const useAssignCase = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...request }: AssignCaseRequest & { id: string }) => 
      casesApi.assign(id, request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['cases'] });
      queryClient.invalidateQueries({ queryKey: ['cases', variables.id] });
    },
  });
};

export const useUnassignCase = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => casesApi.unassign(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['cases'] });
      queryClient.invalidateQueries({ queryKey: ['cases', id] });
    },
  });
};

export const useCloseCase = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => casesApi.close(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['cases'] });
      queryClient.invalidateQueries({ queryKey: ['cases', id] });
    },
  });
};

export const useReopenCase = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => casesApi.reopen(id),
    onSuccess: (_, id) => {
      queryClient.invalidateQueries({ queryKey: ['cases'] });
      queryClient.invalidateQueries({ queryKey: ['cases', id] });
    },
  });
};
