export type AccountStatus = 'Open' | 'Closed' | 'Paid' | 'WrittenOff';

export interface DebtAccount {
  id: string;
  externalId?: string;
  debtorId: string;
  debtorDisplayName: string;
  portfolioId: string;
  portfolioName: string;
  importBatchId?: string;
  accountNumber: string;
  creditorReference?: string;
  originalAmount: number;
  currentBalance: number;
  interestAmount: number;
  feesAmount: number;
  dueDate?: string;
  daysPastDue: number;
  lastPaymentDate?: string;
  lastPaymentAmount?: number;
  status: AccountStatus;
  notes?: string;
  isActive: boolean;
  createdAt: string;
}

export interface DebtAccountListItem {
  id: string;
  debtorId: string;
  debtorDisplayName: string;
  portfolioId: string;
  portfolioName: string;
  accountNumber: string;
  originalAmount: number;
  currentBalance: number;
  status: AccountStatus;
  isActive: boolean;
  createdAt: string;
}

export interface CreateDebtAccountRequest {
  externalId?: string;
  debtorId: string;
  portfolioId: string;
  accountNumber: string;
  creditorReference?: string;
  originalAmount: number;
  currentBalance: number;
  interestAmount?: number;
  feesAmount?: number;
  dueDate?: string;
  daysPastDue?: number;
  notes?: string;
}

export interface UpdateDebtAccountRequest {
  id: string;
  externalId?: string;
  creditorReference?: string;
  currentBalance: number;
  interestAmount?: number;
  feesAmount?: number;
  dueDate?: string;
  daysPastDue?: number;
  status: AccountStatus;
  notes?: string;
}

export interface RecordPaymentRequest {
  amount: number;
  paymentDate: string;
}
