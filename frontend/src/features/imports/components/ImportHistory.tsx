import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo } from 'react';
import { ImportBatchListItem, ImportStatus } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Eye, RotateCcw } from 'lucide-react';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

interface ImportHistoryProps {
  imports: ImportBatchListItem[];
  onView: (importItem: ImportBatchListItem) => void;
  onRollback: (importItem: ImportBatchListItem) => void;
  loading?: boolean;
}

export function ImportHistory({ imports, onView, onRollback, loading }: ImportHistoryProps) {
  const columnDefs = useMemo<ColDef<ImportBatchListItem>[]>(
    () => [
      { 
        field: 'createdAt', 
        headerName: 'Date', 
        width: 120,
        valueFormatter: (params) => new Date(params.value).toLocaleDateString(),
      },
      { field: 'filename', headerName: 'Filename', flex: 1, minWidth: 200 },
      { field: 'portfolioName', headerName: 'Portfolio', width: 150 },
      { field: 'totalRows', headerName: 'Rows', width: 80 },
      { field: 'createdAccounts', headerName: 'Accounts', width: 100 },
      { field: 'createdByName', headerName: 'Imported By', width: 130 },
      {
        field: 'status',
        headerName: 'Status',
        width: 120,
        cellRenderer: (params: ICellRendererParams<ImportBatchListItem, ImportStatus>) => (
          <Badge variant={params.value === 'RolledBack' ? 'destructive' : 'default'}>
            {params.value}
          </Badge>
        ),
      },
      {
        headerName: 'Actions',
        width: 120,
        cellRenderer: (params: ICellRendererParams<ImportBatchListItem>) => (
          <div className="flex gap-1">
            <Button variant="ghost" size="icon" onClick={() => params.data && onView(params.data)}>
              <Eye className="h-4 w-4" />
            </Button>
            {params.data?.status === 'Completed' && (
              <Button 
                variant="ghost" 
                size="icon" 
                onClick={() => params.data && onRollback(params.data)}
              >
                <RotateCcw className="h-4 w-4" />
              </Button>
            )}
          </div>
        ),
      },
    ],
    [onView, onRollback]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<ImportBatchListItem>
        rowData={imports}
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
