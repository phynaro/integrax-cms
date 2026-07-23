export type AuditEventType = 
  | 'Create' 
  | 'Update' 
  | 'Delete' 
  | 'Login' 
  | 'Logout' 
  | 'LoginFailed' 
  | 'RoleChange' 
  | 'UserActivate' 
  | 'UserDeactivate' 
  | 'Export' 
  | 'Import';

export interface AuditEvent {
  id: string;
  eventType: AuditEventType;
  entityType?: string;
  entityId?: string;
  userId?: string;
  userEmail?: string;
  ipAddress?: string;
  userAgent?: string;
  oldValues?: Record<string, unknown>;
  newValues?: Record<string, unknown>;
  metadata?: Record<string, unknown>;
  createdAt: string;
}

export interface AuditFilters {
  page?: number;
  pageSize?: number;
  eventType?: AuditEventType;
  entityType?: string;
  entityId?: string;
  userId?: string;
  fromDate?: string;
  toDate?: string;
}
