import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { debtorsApi } from './api';
import { CreateDebtorRequest, UpdateDebtorRequest, CreateContactRequest, CreateAddressRequest } from './types';

export const useDebtors = (params?: { page?: number; pageSize?: number; search?: string }) => {
  return useQuery({
    queryKey: ['debtors', params],
    queryFn: () => debtorsApi.getAll(params),
  });
};

export const useDebtor = (id: string) => {
  return useQuery({
    queryKey: ['debtors', id],
    queryFn: () => debtorsApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateDebtor = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateDebtorRequest) => debtorsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['debtors'] });
    },
  });
};

export const useUpdateDebtor = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateDebtorRequest) => debtorsApi.update(data),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['debtors'] });
      queryClient.invalidateQueries({ queryKey: ['debtors', variables.id] });
    },
  });
};

export const useDeleteDebtor = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => debtorsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['debtors'] });
    },
  });
};

export const useAddContact = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ debtorId, ...request }: CreateContactRequest & { debtorId: string }) => 
      debtorsApi.addContact(debtorId, request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['debtors', variables.debtorId] });
    },
  });
};

export const useDeleteContact = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ debtorId, contactId }: { debtorId: string; contactId: string }) => 
      debtorsApi.deleteContact(debtorId, contactId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['debtors', variables.debtorId] });
    },
  });
};

export const useAddAddress = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ debtorId, ...request }: CreateAddressRequest & { debtorId: string }) => 
      debtorsApi.addAddress(debtorId, request),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['debtors', variables.debtorId] });
    },
  });
};

export const useDeleteAddress = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ debtorId, addressId }: { debtorId: string; addressId: string }) => 
      debtorsApi.deleteAddress(debtorId, addressId),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['debtors', variables.debtorId] });
    },
  });
};
