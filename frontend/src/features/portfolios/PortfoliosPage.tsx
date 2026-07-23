import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { Plus, Search } from 'lucide-react';
import { PortfoliosGrid } from './components/PortfoliosGrid';
import { PortfolioForm } from './components/PortfolioForm';
import { 
  usePortfolios, 
  useCreatePortfolio, 
  useUpdatePortfolio, 
  useDeletePortfolio,
} from './hooks';
import { useClients } from '../clients/hooks';
import { Portfolio, CreatePortfolioRequest, PortfolioStatus } from './types';

const statusOptions: Array<{ value: PortfolioStatus | 'all'; label: string }> = [
  { value: 'all', label: 'All Statuses' },
  { value: 'Draft', label: 'Draft' },
  { value: 'Active', label: 'Active' },
  { value: 'Closed', label: 'Closed' },
  { value: 'Archived', label: 'Archived' },
];

export function PortfoliosPage() {
  const [search, setSearch] = useState('');
  const [statusFilter, setStatusFilter] = useState<PortfolioStatus | 'all'>('all');
  const [clientFilter, setClientFilter] = useState<string>('all');
  const [formOpen, setFormOpen] = useState(false);
  const [selectedPortfolio, setSelectedPortfolio] = useState<Portfolio | undefined>();

  const { data: portfoliosData, isLoading } = usePortfolios({ 
    search,
    status: statusFilter === 'all' ? undefined : statusFilter,
    clientId: clientFilter === 'all' ? undefined : clientFilter,
  });
  const { data: clientsData } = useClients();
  const createMutation = useCreatePortfolio();
  const updateMutation = useUpdatePortfolio();
  const deleteMutation = useDeletePortfolio();

  const handleCreate = () => {
    setSelectedPortfolio(undefined);
    setFormOpen(true);
  };

  const handleEdit = (portfolio: Portfolio) => {
    setSelectedPortfolio(portfolio);
    setFormOpen(true);
  };

  const handleDelete = async (portfolio: Portfolio) => {
    if (confirm(`Delete portfolio ${portfolio.name}?`)) {
      await deleteMutation.mutateAsync(portfolio.id);
    }
  };

  const handleSubmit = async (formData: CreatePortfolioRequest) => {
    if (selectedPortfolio) {
      await updateMutation.mutateAsync({ ...formData, id: selectedPortfolio.id });
    } else {
      await createMutation.mutateAsync(formData);
    }
    setFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Portfolios</h1>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 h-4 w-4" />
          New Portfolio
        </Button>
      </div>

      <div className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search portfolios..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-8"
          />
        </div>
        
        <Select value={clientFilter} onValueChange={setClientFilter}>
          <SelectTrigger className="w-[200px]">
            <SelectValue placeholder="All Clients" />
          </SelectTrigger>
          <SelectContent>
            <SelectItem value="all">All Clients</SelectItem>
            {clientsData?.data?.map((client) => (
              <SelectItem key={client.id} value={client.id}>
                {client.name}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>

        <Select value={statusFilter} onValueChange={(v) => setStatusFilter(v as PortfolioStatus | 'all')}>
          <SelectTrigger className="w-[150px]">
            <SelectValue placeholder="All Statuses" />
          </SelectTrigger>
          <SelectContent>
            {statusOptions.map((option) => (
              <SelectItem key={option.value} value={option.value}>
                {option.label}
              </SelectItem>
            ))}
          </SelectContent>
        </Select>
      </div>

      <PortfoliosGrid
        portfolios={portfoliosData?.data || []}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <PortfolioForm
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
        portfolio={selectedPortfolio}
        clients={clientsData?.data || []}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}
