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
import { User, Role, CreateUserRequest } from '../types';
import { useEffect } from 'react';

const userSchema = z.object({
  email: z.string().email('Invalid email address'),
  firstName: z.string().min(1, 'First name is required'),
  lastName: z.string().min(1, 'Last name is required'),
  displayName: z.string().optional(),
  roleId: z.string().min(1, 'Role is required'),
});

interface UserFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateUserRequest) => void;
  user?: User;
  roles: Role[];
  loading?: boolean;
}

export function UserForm({ open, onClose, onSubmit, user, roles, loading }: UserFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
    setValue,
    watch,
  } = useForm<CreateUserRequest>({
    resolver: zodResolver(userSchema),
    defaultValues: {
      email: '',
      firstName: '',
      lastName: '',
      displayName: '',
      roleId: '',
    },
  });

  const selectedRoleId = watch('roleId');

  useEffect(() => {
    if (user) {
      reset({
        email: user.email,
        firstName: user.firstName,
        lastName: user.lastName,
        displayName: user.displayName || '',
        roleId: user.roleId,
      });
    } else {
      reset({
        email: '',
        firstName: '',
        lastName: '',
        displayName: '',
        roleId: '',
      });
    }
  }, [user, reset]);

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Sheet open={open} onOpenChange={(next) => { if (!next) handleClose() }}>
      <SheetContent className="sm:max-w-lg">
        <SheetHeader>
          <SheetTitle>{user ? 'Edit User' : 'New User'}</SheetTitle>
        </SheetHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="space-y-2">
            <Label htmlFor="email">Email *</Label>
            <Input 
              id="email" 
              type="email" 
              {...register('email')} 
              placeholder="user@example.com"
              disabled={!!user}
            />
            {errors.email && <p className="text-sm text-destructive">{errors.email.message}</p>}
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="firstName">First Name *</Label>
              <Input id="firstName" {...register('firstName')} placeholder="John" />
              {errors.firstName && (
                <p className="text-sm text-destructive">{errors.firstName.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="lastName">Last Name *</Label>
              <Input id="lastName" {...register('lastName')} placeholder="Doe" />
              {errors.lastName && (
                <p className="text-sm text-destructive">{errors.lastName.message}</p>
              )}
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="displayName">Display Name</Label>
            <Input id="displayName" {...register('displayName')} placeholder="Optional display name" />
          </div>

          <div className="space-y-2">
            <Label htmlFor="roleId">Role *</Label>
            <Select
              value={selectedRoleId}
              onValueChange={(value) => setValue('roleId', value)}
            >
              <SelectTrigger>
                <SelectValue placeholder="Select a role" />
              </SelectTrigger>
              <SelectContent>
                {roles.map((role) => (
                  <SelectItem key={role.id} value={role.id}>
                    {role.name}
                    {role.description && (
                      <span className="text-muted-foreground ml-2">- {role.description}</span>
                    )}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            {errors.roleId && <p className="text-sm text-destructive">{errors.roleId.message}</p>}
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
