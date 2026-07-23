import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Portfolio, CreatePortfolioRequest, PortfolioStatus } from '../types';
import { Client } from '../../clients/types';
import { useEffect } from 'react';

const portfolioSchema = z.object({
  externalId: z.string().optional(),
  clientId: z.string().min(1, 'Client is required'),
  name: z.string().min(1, 'Name is required'),
  code: z.string().min(1, 'Code is required').max(50),
  description: z.string().optional(),
  receivedDate: z.string().optional(),
  status: z.enum(['Draft', 'Active', 'Closed', 'Archived']).optional(),
});

interface PortfolioFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreatePortfolioRequest) => void;
  portfolio?: Portfolio;
  clients: Client[];
  loading?: boolean;
}

const statusOptions: PortfolioStatus[] = ['Draft', 'Active', 'Closed', 'Archived'];

export function PortfolioForm({ open, onClose, onSubmit, portfolio, clients, loading }: PortfolioFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
  } = useForm<CreatePortfolioRequest>({
    resolver: zodResolver(portfolioSchema),
    defaultValues: {
      externalId: '',
      clientId: '',
      name: '',
      code: '',
      description: '',
      receivedDate: '',
      status: 'Draft',
    },
  });

  const selectedClientId = watch('clientId');
  const selectedStatus = watch('status');

  useEffect(() => {
    if (portfolio) {
      reset({
        externalId: portfolio.externalId || '',
        clientId: portfolio.clientId,
        name: portfolio.name,
        code: portfolio.code,
        description: portfolio.description || '',
        receivedDate: portfolio.receivedDate || '',
        status: portfolio.status,
      });
    } else {
      reset({
        externalId: '',
        clientId: '',
        name: '',
        code: '',
        description: '',
        receivedDate: '',
        status: 'Draft',
      });
    }
  }, [portfolio, reset]);

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Sheet open={open} onOpenChange={handleClose}>
      <SheetContent className="sm:max-w-lg">
        <SheetHeader>
          <SheetTitle>{portfolio ? 'Edit Portfolio' : 'New Portfolio'}</SheetTitle>
        </SheetHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
          <div className="space-y-2">
            <Label htmlFor="clientId">Client *</Label>
            <Select
              value={selectedClientId}
              onValueChange={(value) => setValue('clientId', value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select a client" />
              </SelectTrigger>
              <SelectContent>
                {clients.map((client) => (
                  <SelectItem key={client.id} value={client.id}>
                    {client.name} ({client.code})
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.clientId && <p className="text-sm text-destructive">{errors.clientId.message}</p>}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="code">Code *</Label>
              <Input id="code" {...register('code')} placeholder="BANK-ABC-2026-Q1" />
              {errors.code && <p className="text-sm text-destructive">{errors.code.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="externalId">External ID</Label>
              <Input id="externalId" {...register('externalId')} />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input id="name" {...register('name')} placeholder="Portfolio name" />
            {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="description">Description</Label>
            <Input id="description" {...register('description')} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="receivedDate">Received Date</Label>
              <Input id="receivedDate" type="date" {...register('receivedDate')} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="status">Status</Label>
              <Select
                value={selectedStatus}
                onValueChange={(value) => setValue('status', value as PortfolioStatus)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select status" />
                </SelectTrigger>
                <SelectContent>
                  {statusOptions.map((status) => (
                    <SelectItem key={status} value={status}>
                      {status}
                    </SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
          </div>

          <div className="flex justify-end gap-2 pt-4">
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? 'Saving...' : 'Save'}
            </Button>
          </div>
        </form>
      </SheetContent>
    </Sheet>
  );
}
