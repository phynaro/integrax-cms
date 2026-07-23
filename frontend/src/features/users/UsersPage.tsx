import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Plus, Search } from 'lucide-react';
import { UsersGrid } from './components/UsersGrid';
import { UserForm } from './components/UserForm';
import { 
  useUsers, 
  useRoles, 
  useCreateUser, 
  useUpdateUser, 
  useDeleteUser,
  useActivateUser,
  useDeactivateUser,
} from './hooks';
import { User, CreateUserRequest } from './types';

export function UsersPage() {
  const [search, setSearch] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [selectedUser, setSelectedUser] = useState<User | undefined>();

  const { data: usersData, isLoading } = useUsers({ search });
  const { data: rolesData } = useRoles();
  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser();
  const deleteMutation = useDeleteUser();
  const activateMutation = useActivateUser();
  const deactivateMutation = useDeactivateUser();

  const handleCreate = () => {
    setSelectedUser(undefined);
    setFormOpen(true);
  };

  const handleEdit = (user: User) => {
    setSelectedUser(user);
    setFormOpen(true);
  };

  const handleDelete = async (user: User) => {
    if (confirm(`Delete user ${user.email}?`)) {
      await deleteMutation.mutateAsync(user.id);
    }
  };

  const handleToggleActive = async (user: User) => {
    const action = user.isActive ? 'deactivate' : 'activate';
    if (confirm(`${action.charAt(0).toUpperCase() + action.slice(1)} user ${user.email}?`)) {
      if (user.isActive) {
        await deactivateMutation.mutateAsync(user.id);
      } else {
        await activateMutation.mutateAsync(user.id);
      }
    }
  };

  const handleSubmit = async (formData: CreateUserRequest) => {
    if (selectedUser) {
      await updateMutation.mutateAsync({ ...formData, id: selectedUser.id });
    } else {
      await createMutation.mutateAsync(formData);
    }
    setFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Users</h1>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 h-4 w-4" />
          New User
        </Button>
      </div>

      <div className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search users..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-8"
          />
        </div>
      </div>

      <UsersGrid
        users={usersData?.data || []}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onToggleActive={handleToggleActive}
      />

      <UserForm
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
        user={selectedUser}
        roles={rolesData?.data || []}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}
