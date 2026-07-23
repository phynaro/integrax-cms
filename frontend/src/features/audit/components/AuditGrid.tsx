import { AgGridReact } from 'ag-grid-react';
import { ColDef, ICellRendererParams } from 'ag-grid-community';
import { useMemo, useState } from 'react';
import { AuditEvent, AuditEventType } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Eye } from 'lucide-react';

interface AuditGridProps {
  events: AuditEvent[];
  loading?: boolean;
}

const eventTypeColors: Record<AuditEventType, 'default' | 'secondary' | 'destructive' | 'outline'> = {
  Create: 'default',
  Update: 'secondary',
  Delete: 'destructive',
  Login: 'outline',
  Logout: 'outline',
  LoginFailed: 'destructive',
  RoleChange: 'secondary',
  UserActivate: 'default',
  UserDeactivate: 'destructive',
  Export: 'outline',
  Import: 'outline',
};

export function AuditGrid({ events, loading }: AuditGridProps) {
  const [selectedEvent, setSelectedEvent] = useState<AuditEvent | null>(null);

  const columnDefs = useMemo<ColDef<AuditEvent>[]>(
    () => [
      {
        field: 'createdAt',
        headerName: 'Timestamp',
        width: 180,
        valueFormatter: (params) => 
          params.value ? new Date(params.value).toLocaleString() : '-',
        sort: 'desc',
      },
      {
        field: 'eventType',
        headerName: 'Event',
        width: 130,
        cellRenderer: (params: ICellRendererParams<AuditEvent, AuditEventType>) => (
          <Badge variant={eventTypeColors[params.value || 'Create']}>
            {params.value}
          </Badge>
        ),
      },
      { field: 'entityType', headerName: 'Entity Type', width: 120 },
      { 
        field: 'entityId', 
        headerName: 'Entity ID', 
        width: 150,
        valueFormatter: (params) => params.value ? params.value.substring(0, 8) + '...' : '-',
      },
      { field: 'userEmail', headerName: 'User', flex: 1, minWidth: 200 },
      { field: 'ipAddress', headerName: 'IP Address', width: 130 },
      {
        headerName: 'Details',
        width: 100,
        cellRenderer: (params: ICellRendererParams<AuditEvent>) => (
          <Button 
            variant="ghost" 
            size="icon" 
            onClick={() => params.data && setSelectedEvent(params.data)}
          >
            <Eye className="h-4 w-4" />
          </Button>
        ),
      },
    ],
    []
  );

  return (
    <>
      <div className="ag-theme-alpine h-[600px] w-full">
        <AgGridReact<AuditEvent>
          rowData={events}
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

      <Dialog open={!!selectedEvent} onOpenChange={() => setSelectedEvent(null)}>
        <DialogContent className="max-w-2xl max-h-[80vh] overflow-auto">
          <DialogHeader>
            <DialogTitle>Audit Event Details</DialogTitle>
          </DialogHeader>
          {selectedEvent && (
            <div className="space-y-4">
              <div className="grid grid-cols-2 gap-4 text-sm">
                <div>
                  <span className="font-medium text-muted-foreground">Event Type:</span>
                  <p>{selectedEvent.eventType}</p>
                </div>
                <div>
                  <span className="font-medium text-muted-foreground">Timestamp:</span>
                  <p>{new Date(selectedEvent.createdAt).toLocaleString()}</p>
                </div>
                <div>
                  <span className="font-medium text-muted-foreground">Entity Type:</span>
                  <p>{selectedEvent.entityType || '-'}</p>
                </div>
                <div>
                  <span className="font-medium text-muted-foreground">Entity ID:</span>
                  <p className="font-mono text-xs">{selectedEvent.entityId || '-'}</p>
                </div>
                <div>
                  <span className="font-medium text-muted-foreground">User:</span>
                  <p>{selectedEvent.userEmail || '-'}</p>
                </div>
                <div>
                  <span className="font-medium text-muted-foreground">IP Address:</span>
                  <p>{selectedEvent.ipAddress || '-'}</p>
                </div>
              </div>

              {selectedEvent.oldValues && (
                <div>
                  <span className="font-medium text-muted-foreground">Old Values:</span>
                  <pre className="mt-1 p-2 bg-muted rounded text-xs overflow-auto">
                    {JSON.stringify(selectedEvent.oldValues, null, 2)}
                  </pre>
                </div>
              )}

              {selectedEvent.newValues && (
                <div>
                  <span className="font-medium text-muted-foreground">New Values:</span>
                  <pre className="mt-1 p-2 bg-muted rounded text-xs overflow-auto">
                    {JSON.stringify(selectedEvent.newValues, null, 2)}
                  </pre>
                </div>
              )}

              {selectedEvent.userAgent && (
                <div>
                  <span className="font-medium text-muted-foreground">User Agent:</span>
                  <p className="text-xs text-muted-foreground mt-1">{selectedEvent.userAgent}</p>
                </div>
              )}
            </div>
          )}
        </DialogContent>
      </Dialog>
    </>
  );
}
