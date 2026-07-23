import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { DebtorListItem } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Eye, Pencil, Trash2 } from 'lucide-react';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

interface DebtorsGridProps {
  debtors: DebtorListItem[];
  onView: (debtor: DebtorListItem) => void;
  onEdit: (debtor: DebtorListItem) => void;
  onDelete: (debtor: DebtorListItem) => void;
  loading?: boolean;
}

export function DebtorsGrid({ debtors, onView, onEdit, onDelete, loading }: DebtorsGridProps) {
  const columnDefs = useMemo<ColDef<DebtorListItem>[]>(
    () => [
      { field: 'externalId', headerName: 'External ID', width: 130 },
      { field: 'displayName', headerName: 'Name', flex: 1, minWidth: 200 },
      {
        field: 'debtorType',
        headerName: 'Type',
        width: 110,
        cellRenderer: (params: ICellRendererParams<DebtorListItem, string>) => (
          <Badge variant={params.value === 'Company' ? 'secondary' : 'outline'}>
            {params.value}
          </Badge>
        ),
      },
      { field: 'contactCount', headerName: 'Contacts', width: 100 },
      { field: 'accountCount', headerName: 'Accounts', width: 100 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 100,
        cellRenderer: (params: ICellRendererParams<DebtorListItem, boolean>) => (
          <Badge variant={params.value ? 'default' : 'secondary'}>
            {params.value ? 'Active' : 'Inactive'}
          </Badge>
        ),
      },
      {
        headerName: 'Actions',
        width: 150,
        cellRenderer: (params: ICellRendererParams<DebtorListItem>) => (
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
      <AgGridReact<DebtorListItem>
        rowData={debtors}
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
