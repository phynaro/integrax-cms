import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { accountsApi } from './api';
import { CreateDebtAccountRequest, UpdateDebtAccountRequest, RecordPaymentRequest, AccountStatus } from './types';

export const useAccounts = (params?: { 
  page?: number; 
  pageSize?: number; 
  portfolioId?: string;
  debtorId?: string;
  status?: AccountStatus;
  search?: string;
}) => {
  return useQuery({
    queryKey: ['accounts', params],
    queryFn: () => accountsApi.getAll(params),
  });
};

export const useAccount = (id: string) => {
  return useQuery({
    queryKey: ['accounts', id],
    queryFn: () => accountsApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateAccount = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDebtAccountRequest) => accountsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};

export const useUpdateAccount = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateDebtAccountRequest) => accountsApi.update(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['accounts', variables.id] });
    },
  });
};

export const useDeleteAccount = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => accountsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
    },
  });
};

export const useRecordPayment = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ id, ...request }: RecordPaymentRequest & { id: string }) => 
      accountsApi.recordPayment(id, request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['accounts', variables.id] });
    },
  });
};
