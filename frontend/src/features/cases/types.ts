export type CaseStatus = 'New' | 'InProgress' | 'Pending' | 'Closed' | 'Cancelled';
export type CasePriority = 'Low' | 'Medium' | 'High';

export interface CollectionCase {
  id: string;
  caseNumber: string;
  debtAccountId: string;
  accountNumber: string;
  debtorId: string;
  debtorDisplayName: string;
  currentBalance: number;
  assignedToId?: string;
  assignedToName?: string;
  status: CaseStatus;
  priority: CasePriority;
  openedAt: string;
  closedAt?: string;
  notes?: string;
  createdAt: string;
}

export interface CaseListItem {
  id: string;
  caseNumber: string;
  debtorDisplayName: string;
  accountNumber: string;
  currentBalance: number;
  assignedToName?: string;
  status: CaseStatus;
  priority: CasePriority;
  openedAt: string;
}

export interface UpdateCaseRequest {
  id: string;
  status: CaseStatus;
  priority: CasePriority;
  notes?: string;
}

export interface AssignCaseRequest {
  assignedToId: string;
}
