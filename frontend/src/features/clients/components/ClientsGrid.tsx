import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { Client } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Pencil, Trash2 } from 'lucide-react';

interface ClientsGridProps {
  clients: Client[];
  onEdit: (client: Client) => void;
  onDelete: (client: Client) => void;
  loading?: boolean;
}

export function ClientsGrid({ clients, onEdit, onDelete, loading }: ClientsGridProps) {
  const columnDefs = useMemo<ColDef<Client>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 120 },
      { field: 'name', headerName: 'Name', flex: 1, minWidth: 200 },
      { field: 'contactName', headerName: 'Contact', width: 150 },
      { field: 'contactEmail', headerName: 'Email', width: 200 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 100,
        cellRenderer: (params: ICellRendererParams<Client, boolean>) => (
          <Badge variant={params.value ? 'default' : 'secondary'}>
            {params.value ? 'Active' : 'Inactive'}
          </Badge>
        ),
      },
      {
        headerName: 'Actions',
        width: 120,
        cellRenderer: (params: ICellRendererParams<Client>) => (
          <div className="flex gap-1">
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
    [onEdit, onDelete]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<Client>
        rowData={clients}
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
