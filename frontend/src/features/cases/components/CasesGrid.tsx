import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { CaseListItem, CaseStatus, CasePriority } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Eye } from 'lucide-react';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

interface CasesGridProps {
  cases: CaseListItem[];
  onView: (caseItem: CaseListItem) => void;
  loading?: boolean;
}

const statusVariant: Record<CaseStatus, 'default' | 'secondary' | 'outline' | 'destructive'> = {
  New: 'default',
  InProgress: 'secondary',
  Pending: 'outline',
  Closed: 'outline',
  Cancelled: 'destructive',
};

const priorityVariant: Record<CasePriority, 'default' | 'secondary' | 'destructive'> = {
  Low: 'secondary',
  Medium: 'default',
  High: 'destructive',
};

const formatCurrency = (value: number) => 
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

export function CasesGrid({ cases, onView, loading }: CasesGridProps) {
  const columnDefs = useMemo<ColDef<CaseListItem>[]>(
    () => [
      { field: 'caseNumber', headerName: 'Case #', width: 160 },
      { field: 'debtorDisplayName', headerName: 'Debtor', flex: 1, minWidth: 180 },
      { field: 'accountNumber', headerName: 'Account #', width: 140 },
      { 
        field: 'currentBalance', 
        headerName: 'Balance', 
        width: 120,
        valueFormatter: (params) => formatCurrency(params.value),
      },
      { field: 'assignedToName', headerName: 'Assigned To', width: 130 },
      {
        field: 'status',
        headerName: 'Status',
        width: 110,
        cellRenderer: (params: ICellRendererParams<CaseListItem, CaseStatus>) => (
          <Badge variant={statusVariant[params.value || 'New']}>
            {params.value}
          </Badge>
        ),
      },
      {
        field: 'priority',
        headerName: 'Priority',
        width: 100,
        cellRenderer: (params: ICellRendererParams<CaseListItem, CasePriority>) => (
          <Badge variant={priorityVariant[params.value || 'Medium']}>
            {params.value}
          </Badge>
        ),
      },
      {
        headerName: 'Actions',
        width: 80,
        cellRenderer: (params: ICellRendererParams<CaseListItem>) => (
          <Button variant="ghost" size="icon" onClick={() => params.data && onView(params.data)}>
            <Eye className="h-4 w-4" />
          </Button>
        ),
      },
    ],
    [onView]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<CaseListItem>
        rowData={cases}
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
