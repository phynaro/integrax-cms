import { DebtAccount } from '../types';
import { Badge } from '@/components/ui/badge';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { DollarSign, Calendar, FileText } from 'lucide-react';

interface AccountDetailProps {
  open: boolean;
  onClose: () => void;
  account: DebtAccount | null;
}

const formatCurrency = (value: number) => 
  new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD' }).format(value);

const formatDate = (dateStr?: string) => {
  if (!dateStr) return 'N/A';
  return new Date(dateStr).toLocaleDateString();
};

export function AccountDetail({ open, onClose, account }: AccountDetailProps) {
  if (!account) return null;

  return (
    <Sheet open={open} onOpenChange={onClose}>
      <SheetContent className="sm:max-w-xl overflow-y-auto">
        <SheetHeader>
          <SheetTitle className="flex items-center gap-2">
            <FileText className="h-5 w-5" />
            Account {account.accountNumber}
          </SheetTitle>
        </SheetHeader>

        <div className="mt-6 space-y-6">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <p className="text-sm text-muted-foreground">Debtor</p>
              <p className="font-medium">{account.debtorDisplayName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Portfolio</p>
              <p className="font-medium">{account.portfolioName}</p>
            </div>
            <div>
              <p className="text-sm text-muted-foreground">Status</p>
              <Badge>{account.status}</Badge>
            </div>
            {account.creditorReference && (
              <div>
                <p className="text-sm text-muted-foreground">Creditor Ref</p>
                <p className="font-medium">{account.creditorReference}</p>
              </div>
            )}
          </div>

          <div className="space-y-4 p-4 bg-muted/50 rounded-none">
            <h3 className="font-semibold flex items-center gap-2">
              <DollarSign className="h-4 w-4" />
              Financial Details
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Original Amount</p>
                <p className="text-lg font-bold">{formatCurrency(account.originalAmount)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Current Balance</p>
                <p className="text-lg font-bold text-primary">{formatCurrency(account.currentBalance)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Interest</p>
                <p className="font-medium">{formatCurrency(account.interestAmount)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Fees</p>
                <p className="font-medium">{formatCurrency(account.feesAmount)}</p>
              </div>
            </div>
          </div>

          <div className="space-y-4 p-4 bg-muted/50 rounded-none">
            <h3 className="font-semibold flex items-center gap-2">
              <Calendar className="h-4 w-4" />
              Payment History
            </h3>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <p className="text-sm text-muted-foreground">Due Date</p>
                <p className="font-medium">{formatDate(account.dueDate)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Days Past Due</p>
                <p className="font-medium">{account.daysPastDue}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Last Payment Date</p>
                <p className="font-medium">{formatDate(account.lastPaymentDate)}</p>
              </div>
              <div>
                <p className="text-sm text-muted-foreground">Last Payment Amount</p>
                <p className="font-medium">
                  {account.lastPaymentAmount ? formatCurrency(account.lastPaymentAmount) : 'N/A'}
                </p>
              </div>
            </div>
          </div>

          {account.notes && (
            <div>
              <p className="text-sm text-muted-foreground">Notes</p>
              <p className="font-medium">{account.notes}</p>
            </div>
          )}
        </div>
      </SheetContent>
    </Sheet>
  );
}
