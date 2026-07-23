import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { DebtAccountListItem, AccountStatus } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Eye, Pencil, Trash2 } from 'lucide-react';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

interface AccountsGridProps {
  accounts: DebtAccountListItem[];
  onView: (account: DebtAccountListItem) => void;
  onEdit: (account: DebtAccountListItem) => void;
  onDelete: (account: DebtAccountListItem) => void;
  loading?: boolean;
}

const statusVariant: Record<AccountStatus, 'default' | 'secondary' | 'outline' | 'destructive'> = {
  Open: 'default',
  Closed: 'secondary',
  Paid: 'outline',
  WrittenOff: 'destructive',
};

const formatCurrency = (value: number) => 
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

export function AccountsGrid({ accounts, onView, onEdit, onDelete, loading }: AccountsGridProps) {
  const columnDefs = useMemo<ColDef<DebtAccountListItem>[]>(
    () => [
      { field: 'accountNumber', headerName: 'Account #', width: 140 },
      { field: 'debtorDisplayName', headerName: 'Debtor', flex: 1, minWidth: 180 },
      { field: 'portfolioName', headerName: 'Portfolio', width: 150 },
      { 
        field: 'originalAmount', 
        headerName: 'Original', 
        width: 120,
        valueFormatter: (params) => formatCurrency(params.value),
      },
      { 
        field: 'currentBalance', 
        headerName: 'Balance', 
        width: 120,
        valueFormatter: (params) => formatCurrency(params.value),
      },
      {
        field: 'status',
        headerName: 'Status',
        width: 110,
        cellRenderer: (params: ICellRendererParams<DebtAccountListItem, AccountStatus>) => (
          <Badge variant={statusVariant[params.value || 'Open']}>
            {params.value}
          </Badge>
        ),
      },
      {
        headerName: 'Actions',
        width: 140,
        cellRenderer: (params: ICellRendererParams<DebtAccountListItem>) => (
          <div className="flex gap-1">
            <Button variant="ghost" size="icon" onClick={() => params.data && onView(params.data)}>
              <Eye className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="icon" onClick={() => params.data && onEdit(params.data)}>
              <Pencil className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="icon" onClick={() => params.data && onDelete(params.data)}>
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ),
      },
    ],
    [onView, onEdit, onDelete]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<DebtAccountListItem>
        rowData={accounts}
        columnDefs={columnDefs}
        loading={loading}
        pagination
        paginationPageSize={20}
        defaultColDef={{
          sortable: true,
          filter: true,
          resizable: true,
        }}
      />
    </div>
  );
}
