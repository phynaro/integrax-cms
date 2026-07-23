import { CollectionCase } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Briefcase, User, Calendar, XCircle, RotateCcw } from 'lucide-react';

interface CaseDetailProps {
  open: boolean;
  onClose: () => void;
  caseData: CollectionCase | null;
  onClose_: () => void;
  onReopen: () => void;
  loading?: boolean;
}

const formatCurrency = (value: number) => 
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateStr?: string) => {
  if (!dateStr) return 'N/A';
  return new Date(dateStr).toLocaleDateString();
};

export function CaseDetail({ open, onClose, caseData, onClose_, onReopen, loading }: CaseDetailProps) {
  if (!caseData) return null;

  const isClosed = caseData.status === 'Closed';

  return (
    <Sheet open={open} onOpenChange={onClose}>
      <SheetContent className="sm:max-w-xl overflow-y-auto">
        <SheetHeader>
          <SheetTitle className="flex items-center gap-2">
            <Briefcase className="h-5 w-5" />
            Case {caseData.caseNumber}
          </SheetTitle>
        </SheetHeader>

        <div className="mt-6 space-y-6">
          <div className="flex gap-2">
            <Badge>{caseData.status}</Badge>
            <Badge variant={caseData.priority === 'High' ? 'destructive' : 'secondary'}>
              {caseData.priority} Priority
            </Badge>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Debtor</p>
              <p className="font-medium">{caseData.debtorDisplayName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Account Number</p>
              <p className="font-medium">{caseData.accountNumber}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Current Balance</p>
              <p className="text-lg font-bold text-primary">{formatCurrency(caseData.currentBalance)}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground flex items-center gap-1">
                <User className="h-3 w-3" />
                Assigned To
              </p>
              <p className="font-medium">{caseData.assignedToName || 'Unassigned'}</p>
            </div>
          </div>

          <div className="space-y-4 p-4 bg-muted/50 rounded-lg">
            <h3 className="font-semibold flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Timeline
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Opened</p>
                <p className="font-medium">{formatDate(caseData.openedAt)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Closed</p>
                <p className="font-medium">{formatDate(caseData.closedAt)}</p>
              </div>
            </div>
          </div>

          {caseData.notes && (
            <div>
              <p className="text-sm text-muted-foreground">Notes</p>
              <p className="font-medium">{caseData.notes}</p>
            </div>
          )}

          <div className="flex gap-2 pt-4">
            {!isClosed ? (
              <Button onClick={onClose_} disabled={loading} variant="outline">
                <XCircle className="h-4 w-4 mr-2" />
                Close Case
              </Button>
            ) : (
              <Button onClick={onReopen} disabled={loading} variant="outline">
                <RotateCcw className="h-4 w-4 mr-2" />
                Reopen Case
              </Button>
            )}
          </div>
        </div>
      </SheetContent>
    </Sheet>
  );
}
