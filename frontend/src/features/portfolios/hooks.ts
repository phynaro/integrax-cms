import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { portfoliosApi } from './api';
import { CreatePortfolioRequest, UpdatePortfolioRequest, ChangeStatusRequest, PortfolioStatus } from './types';

export const usePortfolios = (params?: { 
  page?: number; 
  pageSize?: number; 
  search?: string;
  clientId?: string;
  status?: PortfolioStatus;
}) => {
  return useQuery({
    queryKey: ['portfolios', params],
    queryFn: () => portfoliosApi.getAll(params),
  });
};

export const usePortfolio = (id: string) => {
  return useQuery({
    queryKey: ['portfolios', id],
    queryFn: () => portfoliosApi.getById(id),
    enabled: !!id,
  });
};

export const usePortfoliosByClient = (clientId: string) => {
  return useQuery({
    queryKey: ['portfolios', 'client', clientId],
    queryFn: () => portfoliosApi.getByClientId(clientId),
    enabled: !!clientId,
  });
};

export const useCreatePortfolio = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreatePortfolioRequest) => portfoliosApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portfolios'] });
    },
  });
};

export const useUpdatePortfolio = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdatePortfolioRequest) => portfoliosApi.update(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portfolios'] });
    },
  });
};

export const useDeletePortfolio = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => portfoliosApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portfolios'] });
    },
  });
};

export const useChangePortfolioStatus = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: ChangeStatusRequest) => portfoliosApi.changeStatus(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['portfolios'] });
    },
  });
};
