import { useState } from 'react';
import { createRouter, createRootRoute, createRoute, Outlet } from '@tanstack/react-router';
import { MainLayout } from '@/components/layout/MainLayout';
import { AuthCallback, ProtectedRoute } from '@/features/auth';
import { ClientsGrid, ClientForm, useClients, useCreateClient, useUpdateClient, useDeleteClient, Client } from '@/features/clients';
import { PortfoliosGrid, PortfolioForm, usePortfolios, useCreatePortfolio, useUpdatePortfolio, useDeletePortfolio, Portfolio } from '@/features/portfolios';
import { UsersGrid, UserForm, useUsers, useRoles, useCreateUser, useUpdateUser, useDeleteUser, useActivateUser, useDeactivateUser, User } from '@/features/users';
import { AuditGrid, useAuditEvents } from '@/features/audit';
import { DebtorsGrid, DebtorForm, DebtorDetail, useDebtors, useDebtor, useCreateDebtor, useUpdateDebtor, useDeleteDebtor, DebtorListItem } from '@/features/debtors';
import { AccountsGrid, AccountDetail, useAccounts, useAccount, DebtAccountListItem, DebtAccount } from '@/features/accounts';
import { CasesGrid, CaseDetail, useCases, useCase, useCloseCase, useReopenCase, CaseListItem, CollectionCase } from '@/features/cases';
import { ImportWizard, ImportHistory, useImports, useRollbackImport, ImportBatchListItem } from '@/features/imports';

const rootRoute = createRootRoute({
  component: () => <Outlet />,
});

const authenticatedLayout = createRoute({
  getParentRoute: () => rootRoute,
  id: 'authenticated',
  component: () => (
    <ProtectedRoute>
      <MainLayout />
    </ProtectedRoute>
  ),
});

const indexRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/',
  component: DashboardPage,
});

const clientsRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/clients',
  component: ClientsPage,
});

const portfoliosRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/portfolios',
  component: PortfoliosPage,
});

const usersRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/users',
  component: UsersPage,
});

const auditRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/audit',
  component: AuditPage,
});

const settingsRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/settings',
  component: SettingsPage,
});

const debtorsRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/debtors',
  component: DebtorsPage,
});

const accountsRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/accounts',
  component: AccountsPage,
});

const casesRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/cases',
  component: CasesPage,
});

const importsRoute = createRoute({
  getParentRoute: () => authenticatedLayout,
  path: '/imports',
  component: ImportsPage,
});

const authCallbackRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/auth/callback',
  component: AuthCallback,
});

const notFoundRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '*',
  component: NotFoundPage,
});

const routeTree = rootRoute.addChildren([
  authenticatedLayout.addChildren([
    indexRoute,
    clientsRoute,
    portfoliosRoute,
    debtorsRoute,
    accountsRoute,
    casesRoute,
    importsRoute,
    usersRoute,
    auditRoute,
    settingsRoute,
  ]),
  authCallbackRoute,
  notFoundRoute,
]);

export const router = createRouter({ routeTree });

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router;
  }
}

function DashboardPage() {
  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-3xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Welcome to Debt Collection Management System</p>
      </div>
      <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-4">
        <DashboardCard title="Total Clients" value="0" description="Active clients" />
        <DashboardCard title="Portfolios" value="0" description="Active portfolios" />
        <DashboardCard title="Total Accounts" value="0" description="Debt accounts" />
        <DashboardCard title="Collection Rate" value="0%" description="This month" />
      </div>
    </div>
  );
}

function DashboardCard({ title, value, description }: { title: string; value: string; description: string }) {
  return (
    <div className="rounded-none border bg-card p-6 shadow-sm">
      <h3 className="text-sm font-medium text-muted-foreground">{title}</h3>
      <div className="mt-2 text-3xl font-bold">{value}</div>
      <p className="mt-1 text-xs text-muted-foreground">{description}</p>
    </div>
  );
}

function ClientsPage() {
  const [selectedClient, setSelectedClient] = useState<Client | undefined>(undefined);
  const [isFormOpen, setIsFormOpen] = useState(false);
  
  const { data: response, isLoading } = useClients();
  const clients = response?.data ?? [];
  const createMutation = useCreateClient();
  const updateMutation = useUpdateClient();
  const deleteMutation = useDeleteClient();

  const handleCreate = () => {
    setSelectedClient(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (client: Client) => {
    setSelectedClient(client);
    setIsFormOpen(true);
  };

  const handleDelete = (client: Client) => {
    if (confirm('Are you sure you want to delete this client?')) {
      deleteMutation.mutate(client.id);
    }
  };

  const handleSubmit = (data: Partial<Client>) => {
    if (selectedClient) {
      updateMutation.mutate({ id: selectedClient.id, ...data } as any);
    } else {
      createMutation.mutate(data as any);
    }
    setIsFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Clients</h1>
        <button
          onClick={handleCreate}
          className="px-4 py-2 text-sm font-medium text-white bg-primary rounded-none hover:bg-primary/90"
        >
          Add Client
        </button>
      </div>
      <ClientsGrid
        clients={clients}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />
      <ClientForm
        open={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        client={selectedClient}
        onSubmit={handleSubmit}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}

function PortfoliosPage() {
  const [selectedPortfolio, setSelectedPortfolio] = useState<Portfolio | undefined>(undefined);
  const [isFormOpen, setIsFormOpen] = useState(false);
  
  const { data: portfolioResponse, isLoading } = usePortfolios();
  const portfolios = portfolioResponse?.data ?? [];
  const { data: clientResponse } = useClients();
  const clients = clientResponse?.data ?? [];
  const createMutation = useCreatePortfolio();
  const updateMutation = useUpdatePortfolio();
  const deleteMutation = useDeletePortfolio();

  const handleCreate = () => {
    setSelectedPortfolio(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (portfolio: Portfolio) => {
    setSelectedPortfolio(portfolio);
    setIsFormOpen(true);
  };

  const handleDelete = (portfolio: Portfolio) => {
    if (confirm('Are you sure you want to delete this portfolio?')) {
      deleteMutation.mutate(portfolio.id);
    }
  };

  const handleSubmit = (data: Partial<Portfolio>) => {
    if (selectedPortfolio) {
      updateMutation.mutate({ id: selectedPortfolio.id, ...data } as any);
    } else {
      createMutation.mutate(data as any);
    }
    setIsFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Portfolios</h1>
        <button
          onClick={handleCreate}
          className="px-4 py-2 text-sm font-medium text-white bg-primary rounded-none hover:bg-primary/90"
        >
          Add Portfolio
        </button>
      </div>
      <PortfoliosGrid
        portfolios={portfolios}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />
      <PortfolioForm
        open={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        portfolio={selectedPortfolio}
        clients={clients}
        onSubmit={handleSubmit}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}

function UsersPage() {
  const [selectedUser, setSelectedUser] = useState<User | undefined>(undefined);
  const [isFormOpen, setIsFormOpen] = useState(false);
  
  const { data: userResponse, isLoading } = useUsers();
  const users = userResponse?.data ?? [];
  const { data: roleResponse } = useRoles();
  const roles = roleResponse?.data ?? [];
  const createMutation = useCreateUser();
  const updateMutation = useUpdateUser();
  const deleteMutation = useDeleteUser();
  const activateMutation = useActivateUser();
  const deactivateMutation = useDeactivateUser();

  const handleCreate = () => {
    setSelectedUser(undefined);
    setIsFormOpen(true);
  };

  const handleEdit = (user: User) => {
    setSelectedUser(user);
    setIsFormOpen(true);
  };

  const handleDelete = (user: User) => {
    if (confirm('Are you sure you want to delete this user?')) {
      deleteMutation.mutate(user.id);
    }
  };

  const handleToggleActive = (user: User) => {
    if (user.isActive) {
      deactivateMutation.mutate(user.id);
    } else {
      activateMutation.mutate(user.id);
    }
  };

  const handleSubmit = (data: Partial<User>) => {
    if (selectedUser) {
      updateMutation.mutate({ id: selectedUser.id, ...data } as any);
    } else {
      createMutation.mutate(data as any);
    }
    setIsFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Users</h1>
        <button
          onClick={handleCreate}
          className="px-4 py-2 text-sm font-medium text-white bg-primary rounded-none hover:bg-primary/90"
        >
          Add User
        </button>
      </div>
      <UsersGrid
        users={users}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
        onToggleActive={handleToggleActive}
      />
      <UserForm
        open={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        user={selectedUser}
        roles={roles}
        onSubmit={handleSubmit}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}

function AuditPage() {
  const { data: response, isLoading } = useAuditEvents();
  const events = response?.data ?? [];

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Audit Log</h1>
      <AuditGrid events={events} loading={isLoading} />
    </div>
  );
}

function SettingsPage() {
  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Settings</h1>
      <p className="text-muted-foreground">System settings will be available here.</p>
    </div>
  );
}

function DebtorsPage() {
  const [selectedDebtorItem, setSelectedDebtorItem] = useState<DebtorListItem | null>(null);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  
  const { data: response, isLoading } = useDebtors();
  const debtors = response?.data ?? [];
  const { data: fullDebtor } = useDebtor(selectedDebtorItem?.id || '');
  const createMutation = useCreateDebtor();
  const updateMutation = useUpdateDebtor();
  const deleteMutation = useDeleteDebtor();

  const handleCreate = () => {
    setSelectedDebtorItem(null);
    setIsFormOpen(true);
  };

  const handleView = (debtor: DebtorListItem) => {
    setSelectedDebtorItem(debtor);
    setIsDetailOpen(true);
  };

  const handleEdit = (debtor: DebtorListItem) => {
    setSelectedDebtorItem(debtor);
    setIsFormOpen(true);
  };

  const handleDelete = (debtor: DebtorListItem) => {
    if (confirm('Are you sure you want to deactivate this debtor?')) {
      deleteMutation.mutate(debtor.id);
    }
  };

  const handleSubmit = (data: any) => {
    if (selectedDebtorItem) {
      updateMutation.mutate({ id: selectedDebtorItem.id, ...data });
    } else {
      createMutation.mutate(data);
    }
    setIsFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Debtors</h1>
        <button
          onClick={handleCreate}
          className="px-4 py-2 text-sm font-medium text-white bg-primary rounded-none hover:bg-primary/90"
        >
          Add Debtor
        </button>
      </div>
      <DebtorsGrid
        debtors={debtors}
        loading={isLoading}
        onView={handleView}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />
      <DebtorForm
        open={isFormOpen}
        onClose={() => setIsFormOpen(false)}
        debtor={fullDebtor || undefined}
        onSubmit={handleSubmit}
        loading={createMutation.isPending || updateMutation.isPending}
      />
      <DebtorDetail
        open={isDetailOpen}
        onClose={() => setIsDetailOpen(false)}
        debtor={fullDebtor || null}
      />
    </div>
  );
}

function AccountsPage() {
  const [selectedAccount, setSelectedAccount] = useState<DebtAccount | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  
  const { data: response, isLoading } = useAccounts();
  const accounts = response?.data ?? [];
  const { data: fullAccount } = useAccount(selectedAccount?.id || '');

  const handleView = (account: DebtAccountListItem) => {
    setSelectedAccount(account as any);
    setIsDetailOpen(true);
  };

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Debt Accounts</h1>
      <AccountsGrid
        accounts={accounts}
        loading={isLoading}
        onView={handleView}
        onEdit={handleView}
        onDelete={() => {}}
      />
      <AccountDetail
        open={isDetailOpen}
        onClose={() => setIsDetailOpen(false)}
        account={fullAccount || null}
      />
    </div>
  );
}

function CasesPage() {
  const [selectedCase, setSelectedCase] = useState<CollectionCase | null>(null);
  const [isDetailOpen, setIsDetailOpen] = useState(false);
  
  const { data: response, isLoading } = useCases();
  const cases = response?.data ?? [];
  const { data: fullCase } = useCase(selectedCase?.id || '');
  const closeMutation = useCloseCase();
  const reopenMutation = useReopenCase();

  const handleView = (caseItem: CaseListItem) => {
    setSelectedCase(caseItem as any);
    setIsDetailOpen(true);
  };

  const handleClose = () => {
    if (selectedCase) {
      closeMutation.mutate(selectedCase.id);
    }
  };

  const handleReopen = () => {
    if (selectedCase) {
      reopenMutation.mutate(selectedCase.id);
    }
  };

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-bold">Collection Cases</h1>
      <CasesGrid
        cases={cases}
        loading={isLoading}
        onView={handleView}
      />
      <CaseDetail
        open={isDetailOpen}
        onClose={() => setIsDetailOpen(false)}
        caseData={fullCase || null}
        onClose_={handleClose}
        onReopen={handleReopen}
        loading={closeMutation.isPending || reopenMutation.isPending}
      />
    </div>
  );
}

function ImportsPage() {
  const [activeTab, setActiveTab] = useState<'wizard' | 'history'>('history');
  
  const { data: response, isLoading } = useImports();
  const imports = response?.data ?? [];
  const { data: portfolioResponse } = usePortfolios();
  const portfolios = portfolioResponse?.data ?? [];
  const rollbackMutation = useRollbackImport();

  const handleView = (importItem: ImportBatchListItem) => {
    console.log('View import:', importItem);
  };

  const handleRollback = (importItem: ImportBatchListItem) => {
    if (confirm('Are you sure you want to rollback this import? This will delete all imported data.')) {
      rollbackMutation.mutate(importItem.id);
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Import Management</h1>
        <div className="flex gap-2">
          <button
            onClick={() => setActiveTab('history')}
            className={`px-4 py-2 text-sm font-medium rounded-none ${
              activeTab === 'history' ? 'bg-primary text-white' : 'bg-secondary'
            }`}
          >
            History
          </button>
          <button
            onClick={() => setActiveTab('wizard')}
            className={`px-4 py-2 text-sm font-medium rounded-none ${
              activeTab === 'wizard' ? 'bg-primary text-white' : 'bg-secondary'
            }`}
          >
            New Import
          </button>
        </div>
      </div>
      
      {activeTab === 'history' ? (
        <ImportHistory
          imports={imports}
          loading={isLoading}
          onView={handleView}
          onRollback={handleRollback}
        />
      ) : (
        <ImportWizard
          portfolios={portfolios.map(p => ({ id: p.id, name: p.name }))}
          onComplete={() => setActiveTab('history')}
        />
      )}
    </div>
  );
}

function NotFoundPage() {
  return (
    <div className="min-h-screen flex items-center justify-center bg-background">
      <div className="text-center">
        <h1 className="text-6xl font-bold text-muted-foreground">404</h1>
        <p className="mt-4 text-xl text-muted-foreground">Page not found</p>
        <a href="/" className="mt-6 inline-block text-primary hover:underline">
          Go back home
        </a>
      </div>
    </div>
  );
}
