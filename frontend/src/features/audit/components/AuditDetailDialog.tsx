import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { AuditEvent } from '../types';

interface AuditDetailDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  event: AuditEvent | null;
}

export function AuditDetailDialog({ open, onOpenChange, event }: AuditDetailDialogProps) {
  if (!event) return null;

  const formatJson = (data: Record<string, unknown> | string | null | undefined) => {
    if (!data) return 'N/A';
    if (typeof data === 'string') {
      try {
        return JSON.stringify(JSON.parse(data), null, 2);
      } catch {
        return data;
      }
    }
    return JSON.stringify(data, null, 2);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl max-h-[80vh] overflow-y-auto">
        <DialogHeader>
          <DialogTitle>Audit Event Details</DialogTitle>
        </DialogHeader>
        <div className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="text-sm font-medium text-muted-foreground">Event Type</label>
              <p className="mt-1 font-mono">{event.eventType}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Entity Type</label>
              <p className="mt-1 font-mono">{event.entityType || 'N/A'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Entity ID</label>
              <p className="mt-1 font-mono text-xs">{event.entityId || 'N/A'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">Timestamp</label>
              <p className="mt-1">{new Date(event.createdAt).toLocaleString()}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">User</label>
              <p className="mt-1">{event.userEmail || 'System'}</p>
            </div>
            <div>
              <label className="text-sm font-medium text-muted-foreground">IP Address</label>
              <p className="mt-1 font-mono">{event.ipAddress || 'N/A'}</p>
            </div>
          </div>

          {event.oldValues && (
            <div>
              <label className="text-sm font-medium text-muted-foreground">Previous Values</label>
              <pre className="mt-1 p-3 bg-muted rounded-md text-xs overflow-auto max-h-40">
                {formatJson(event.oldValues)}
              </pre>
            </div>
          )}

          {event.newValues && (
            <div>
              <label className="text-sm font-medium text-muted-foreground">New Values</label>
              <pre className="mt-1 p-3 bg-muted rounded-md text-xs overflow-auto max-h-40">
                {formatJson(event.newValues)}
              </pre>
            </div>
          )}

          {event.metadata && (
            <div>
              <label className="text-sm font-medium text-muted-foreground">Metadata</label>
              <pre className="mt-1 p-3 bg-muted rounded-md text-xs overflow-auto max-h-40">
                {formatJson(event.metadata)}
              </pre>
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
}
