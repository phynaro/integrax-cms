import { DebtorType } from '../debtors/types';

export type ImportStatus = 'Completed' | 'RolledBack';

export interface ImportBatch {
  id: string;
  portfolioId: string;
  portfolioName: string;
  filename: string;
  fileSize?: number;
  totalRows: number;
  createdDebtors: number;
  matchedDebtors: number;
  createdAccounts: number;
  createdCases: number;
  status: ImportStatus;
  errorMessage?: string;
  rolledBackAt?: string;
  rolledBackByName?: string;
  createdAt: string;
  createdByName?: string;
}

export interface ImportBatchListItem {
  id: string;
  portfolioId: string;
  portfolioName: string;
  filename: string;
  totalRows: number;
  createdAccounts: number;
  status: ImportStatus;
  createdAt: string;
  createdByName?: string;
}

export interface ImportRowPreview {
  rowNumber: number;
  isValid: boolean;
  errors: string[];
  externalId?: string;
  debtorType: DebtorType;
  firstName?: string;
  lastName?: string;
  companyName?: string;
  dateOfBirth?: string;
  taxId?: string;
  phone?: string;
  email?: string;
  addressLine1?: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  accountNumber?: string;
  creditorReference?: string;
  originalAmount: number;
  currentBalance: number;
  interestAmount: number;
  feesAmount: number;
  dueDate?: string;
  daysPastDue: number;
  lastPaymentDate?: string;
  lastPaymentAmount?: number;
  accountNotes?: string;
}

export interface ImportValidationResult {
  isValid: boolean;
  filename: string;
  totalRows: number;
  validRows: number;
  errorRows: number;
  rows: ImportRowPreview[];
  errors: { rowNumber: number; field: string; message: string }[];
}

export interface ConfirmImportRequest {
  portfolioId: string;
  filename: string;
  rows: ImportRowPreview[];
}

export interface ImportResult {
  importBatchId: string;
  totalRows: number;
  createdDebtors: number;
  matchedDebtors: number;
  createdAccounts: number;
  createdCases: number;
  success: boolean;
  errorMessage?: string;
}

export interface RollbackResult {
  success: boolean;
  deletedCases: number;
  deletedAccounts: number;
  deletedDebtors: number;
  errorMessage?: string;
}
