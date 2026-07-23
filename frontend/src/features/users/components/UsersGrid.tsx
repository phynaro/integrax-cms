import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { User } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Pencil, Trash2, UserCheck, UserX } from 'lucide-react';

interface UsersGridProps {
  users: User[];
  onEdit: (user: User) => void;
  onDelete: (user: User) => void;
  onToggleActive: (user: User) => void;
  loading?: boolean;
}

export function UsersGrid({ users, onEdit, onDelete, onToggleActive, loading }: UsersGridProps) {
  const columnDefs = useMemo<ColDef<User>[]>(
    () => [
      { field: 'email', headerName: 'Email', flex: 1, minWidth: 200 },
      { 
        field: 'firstName', 
        headerName: 'Name', 
        width: 180,
        valueGetter: (params) => `${params.data?.firstName} ${params.data?.lastName}`.trim()
      },
      { 
        field: 'role.name', 
        headerName: 'Role', 
        width: 130,
        cellRenderer: (params: ICellRendererParams<User>) => (
          <Badge variant="outline">{params.data?.role?.name}</Badge>
        ),
      },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 100,
        cellRenderer: (params: ICellRendererParams<User, boolean>) => (
          <Badge variant={params.value ? 'default' : 'secondary'}>
            {params.value ? 'Active' : 'Inactive'}
          </Badge>
        ),
      },
      {
        field: 'lastLoginAt',
        headerName: 'Last Login',
        width: 160,
        valueFormatter: (params) => 
          params.value ? new Date(params.value).toLocaleString() : 'Never',
      },
      {
        headerName: 'Actions',
        width: 150,
        cellRenderer: (params: ICellRendererParams<User>) => (
          <div className="flex gap-1">
            <Button 
              variant="ghost" 
              size="icon" 
              onClick={() => params.data && onEdit(params.data)}
              title="Edit"
            >
              <Pencil className="h-4 w-4" />
            </Button>
            <Button 
              variant="ghost" 
              size="icon" 
              onClick={() => params.data && onToggleActive(params.data)}
              title={params.data?.isActive ? 'Deactivate' : 'Activate'}
            >
              {params.data?.isActive ? (
                <UserX className="h-4 w-4" />
              ) : (
                <UserCheck className="h-4 w-4" />
              )}
            </Button>
            <Button 
              variant="ghost" 
              size="icon" 
              onClick={() => params.data && onDelete(params.data)}
              title="Delete"
            >
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ),
      },
    ],
    [onEdit, onDelete, onToggleActive]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<User>
        rowData={users}
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
