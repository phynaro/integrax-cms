import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Plus, Search } from 'lucide-react';
import { ClientsGrid } from './components/ClientsGrid';
import { ClientForm } from './components/ClientForm';
import { useClients, useCreateClient, useUpdateClient, useDeleteClient } from './hooks';
import { Client, CreateClientRequest } from './types';

export function ClientsPage() {
  const [search, setSearch] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [selectedClient, setSelectedClient] = useState<Client | undefined>();

  const { data, isLoading } = useClients({ search });
  const createMutation = useCreateClient();
  const updateMutation = useUpdateClient();
  const deleteMutation = useDeleteClient();

  const handleCreate = () => {
    setSelectedClient(undefined);
    setFormOpen(true);
  };

  const handleEdit = (client: Client) => {
    setSelectedClient(client);
    setFormOpen(true);
  };

  const handleDelete = async (client: Client) => {
    if (confirm(`Delete ${client.name}?`)) {
      await deleteMutation.mutateAsync(client.id);
    }
  };

  const handleSubmit = async (formData: CreateClientRequest) => {
    if (selectedClient) {
      await updateMutation.mutateAsync({ ...formData, id: selectedClient.id });
    } else {
      await createMutation.mutateAsync(formData);
    }
    setFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Clients</h1>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 h-4 w-4" />
          New Client
        </Button>
      </div>

      <div className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search clients..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-8"
          />
        </div>
      </div>

      <ClientsGrid
        clients={data?.data || []}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <ClientForm
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
        client={selectedClient}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}
