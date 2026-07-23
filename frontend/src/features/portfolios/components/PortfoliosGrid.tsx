import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { Portfolio, PortfolioStatus } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Pencil, Trash2 } from 'lucide-react';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

interface PortfoliosGridProps {
  portfolios: Portfolio[];
  onEdit: (portfolio: Portfolio) => void;
  onDelete: (portfolio: Portfolio) => void;
  loading?: boolean;
}

const statusColors: Record<PortfolioStatus, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  Draft: 'secondary',
  Active: 'default',
  Closed: 'outline',
  Archived: 'destructive',
};

export function PortfoliosGrid({ portfolios, onEdit, onDelete, loading }: PortfoliosGridProps) {
  const columnDefs = useMemo<ColDef<Portfolio>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 150 },
      { field: 'name', headerName: 'Name', flex: 1, minWidth: 200 },
      { field: 'client.name', headerName: 'Client', width: 180 },
      {
        field: 'status',
        headerName: 'Status',
        width: 110,
        cellRenderer: (params: ICellRendererParams<Portfolio, PortfolioStatus>) => (
          <Badge variant={statusColors[params.value || 'Draft']}>
            {params.value}
          </Badge>
        ),
      },
      {
        field: 'totalAccounts',
        headerName: 'Accounts',
        width: 100,
        type: 'numericColumn',
        valueFormatter: (params) => params.value?.toLocaleString() || '0',
      },
      {
        field: 'totalAmount',
        headerName: 'Total Amount',
        width: 140,
        type: 'numericColumn',
        valueFormatter: (params) => 
          params.value != null 
            ? new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(params.value)
            : '$0.00',
      },
      {
        field: 'receivedDate',
        headerName: 'Received',
        width: 120,
        valueFormatter: (params) => 
          params.value ? new Date(params.value).toLocaleDateString() : '-',
      },
      {
        headerName: 'Actions',
        width: 120,
        cellRenderer: (params: ICellRendererParams<Portfolio>) => (
          <div className="flex gap-1">
            <Button 
              variant="ghost" 
              size="icon" 
              onClick={() => params.data && onEdit(params.data)}
            >
              <Pencil className="h-4 w-4" />
            </Button>
            <Button 
              variant="ghost" 
              size="icon" 
              onClick={() => params.data && onDelete(params.data)}
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ),
      },
    ],
    [onEdit, onDelete]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<Portfolio>
        rowData={portfolios}
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
