import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { importsApi } from './api';
import { ConfirmImportRequest } from './types';

export const useImports = (params?: { page?: number; pageSize?: number; portfolioId?: string }) => {
  return useQuery({
    queryKey: ['imports', params],
    queryFn: () => importsApi.getAll(params),
  });
};

export const useImport = (id: string) => {
  return useQuery({
    queryKey: ['imports', id],
    queryFn: () => importsApi.getById(id),
    enabled: !!id,
  });
};

export const useValidateImport = () => {
  return useMutation({
    mutationFn: ({ portfolioId, file }: { portfolioId: string; file: File }) => 
      importsApi.validate(portfolioId, file),
  });
};

export const useConfirmImport = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (request: ConfirmImportRequest) => importsApi.confirm(request),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['imports'] });
      queryClient.invalidateQueries({ queryKey: ['debtors'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['cases'] });
    },
  });
};

export const useRollbackImport = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => importsApi.rollback(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['imports'] });
      queryClient.invalidateQueries({ queryKey: ['debtors'] });
      queryClient.invalidateQueries({ queryKey: ['accounts'] });
      queryClient.invalidateQueries({ queryKey: ['cases'] });
    },
  });
};

export const useDownloadTemplate = () => {
  return useMutation({
    mutationFn: async () => {
      const blob = await importsApi.getTemplate();
      const url = window.URL.createObjectURL(new Blob([blob]));
      const link = document.createElement('a');
      link.href = url;
      link.setAttribute('download', 'import_template.csv');
      document.body.appendChild(link);
      link.click();
      link.remove();
    },
  });
};
