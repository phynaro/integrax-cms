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
import { Debtor, CreateDebtorRequest, DebtorType } from '../types';
import { useEffect } from 'react';

const debtorSchema = z.object({
  externalId: z.string().min(1, 'External ID is required'),
  debtorType: z.enum(['Individual', 'Company']),
  firstName: z.string().optional(),
  lastName: z.string().optional(),
  companyName: z.string().optional(),
  dateOfBirth: z.string().optional(),
  taxId: z.string().optional(),
  notes: z.string().optional(),
});

interface DebtorFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateDebtorRequest) => void;
  debtor?: Debtor;
  loading?: boolean;
}

export function DebtorForm({ open, onClose, onSubmit, debtor, loading }: DebtorFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    watch,
    setValue,
  } = useForm<CreateDebtorRequest>({
    resolver: zodResolver(debtorSchema),
    defaultValues: {
      externalId: '',
      debtorType: 'Individual',
      firstName: '',
      lastName: '',
      companyName: '',
      dateOfBirth: '',
      taxId: '',
      notes: '',
    },
  });

  const debtorType = watch('debtorType');

  useEffect(() => {
    if (debtor) {
      reset({
        externalId: debtor.externalId,
        debtorType: debtor.debtorType,
        firstName: debtor.firstName || '',
        lastName: debtor.lastName || '',
        companyName: debtor.companyName || '',
        dateOfBirth: debtor.dateOfBirth?.split('T')[0] || '',
        taxId: debtor.taxId || '',
        notes: debtor.notes || '',
      });
    } else {
      reset({
        externalId: '',
        debtorType: 'Individual',
        firstName: '',
        lastName: '',
        companyName: '',
        dateOfBirth: '',
        taxId: '',
        notes: '',
      });
    }
  }, [debtor, reset]);

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Sheet open={open} onOpenChange={(next) => { if (!next) handleClose() }}>
      <SheetContent className="sm:max-w-lg overflow-y-auto">
        <SheetHeader>
          <SheetTitle>{debtor ? 'Edit Debtor' : 'New Debtor'}</SheetTitle>
        </SheetHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="externalId">External ID *</Label>
              <Input id="externalId" {...register('externalId')} />
              {errors.externalId && (
                <p className="text-sm text-destructive">{errors.externalId.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="debtorType">Type *</Label>
              <Select
                value={debtorType}
                onValueChange={(value: DebtorType) => setValue('debtorType', value)}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Select type" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Individual">Individual</SelectItem>
                  <SelectItem value="Company">Company</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>

          {debtorType === 'Individual' ? (
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-2">
                <Label htmlFor="firstName">First Name</Label>
                <Input id="firstName" {...register('firstName')} />
              </div>
              <div className="space-y-2">
                <Label htmlFor="lastName">Last Name</Label>
                <Input id="lastName" {...register('lastName')} />
              </div>
            </div>
          ) : (
            <div className="space-y-2">
              <Label htmlFor="companyName">Company Name</Label>
              <Input id="companyName" {...register('companyName')} />
            </div>
          )}

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="dateOfBirth">Date of Birth</Label>
              <Input id="dateOfBirth" type="date" {...register('dateOfBirth')} />
            </div>
            <div className="space-y-2">
              <Label htmlFor="taxId">Tax ID</Label>
              <Input id="taxId" {...register('taxId')} />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notes</Label>
            <Input id="notes" {...register('notes')} />
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
