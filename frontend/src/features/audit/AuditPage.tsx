import { useState } from 'react';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import { AuditGrid } from './components/AuditGrid';
import { useAuditEvents } from './hooks';
import { AuditEventType } from './types';

const eventTypes: Array<{ value: AuditEventType | 'all'; label: string }> = [
  { value: 'all', label: 'All Events' },
  { value: 'Create', label: 'Create' },
  { value: 'Update', label: 'Update' },
  { value: 'Delete', label: 'Delete' },
  { value: 'Login', label: 'Login' },
  { value: 'Logout', label: 'Logout' },
  { value: 'LoginFailed', label: 'Login Failed' },
  { value: 'RoleChange', label: 'Role Change' },
  { value: 'UserActivate', label: 'User Activate' },
  { value: 'UserDeactivate', label: 'User Deactivate' },
  { value: 'Export', label: 'Export' },
  { value: 'Import', label: 'Import' },
];

const entityTypes: Array<{ value: string; label: string }> = [
  { value: 'all', label: 'All Entities' },
  { value: 'User', label: 'User' },
  { value: 'Client', label: 'Client' },
  { value: 'Portfolio', label: 'Portfolio' },
];

export function AuditPage() {
  const [eventTypeFilter, setEventTypeFilter] = useState<AuditEventType | 'all'>('all');
  const [entityTypeFilter, setEntityTypeFilter] = useState<string>('all');
  const [fromDate, setFromDate] = useState('');
  const [toDate, setToDate] = useState('');

  const { data, isLoading } = useAuditEvents({
    eventType: eventTypeFilter === 'all' ? undefined : eventTypeFilter,
    entityType: entityTypeFilter === 'all' ? undefined : entityTypeFilter,
    fromDate: fromDate || undefined,
    toDate: toDate || undefined,
  });

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Audit Log</h1>
      </div>

      <div className="flex flex-wrap items-end gap-4">
        <div className="space-y-1">
          <Label className="text-xs">Event Type</Label>
          <Select 
            value={eventTypeFilter} 
            onValueChange={(v) => setEventTypeFilter(v as AuditEventType | 'all')}
          >
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="All Events" />
            </SelectTrigger>
            <SelectContent>
              {eventTypes.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1">
          <Label className="text-xs">Entity Type</Label>
          <Select value={entityTypeFilter} onValueChange={setEntityTypeFilter}>
            <SelectTrigger className="w-[150px]">
              <SelectValue placeholder="All Entities" />
            </SelectTrigger>
            <SelectContent>
              {entityTypes.map((option) => (
                <SelectItem key={option.value} value={option.value}>
                  {option.label}
                </SelectItem>
              ))}
            </SelectContent>
          </Select>
        </div>

        <div className="space-y-1">
          <Label className="text-xs">From Date</Label>
          <Input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            className="w-[150px]"
          />
        </div>

        <div className="space-y-1">
          <Label className="text-xs">To Date</Label>
          <Input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            className="w-[150px]"
          />
        </div>
      </div>

      <AuditGrid
        events={data?.data || []}
        loading={isLoading}
      />
    </div>
  );
}
