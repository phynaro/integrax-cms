import { Client } from '../clients/types';

export type PortfolioStatus = 'Draft' | 'Active' | 'Closed' | 'Archived';

export interface Portfolio {
  id: string;
  externalId?: string;
  clientId: string;
  client: Client;
  name: string;
  code: string;
  description?: string;
  receivedDate?: string;
  status: PortfolioStatus;
  totalAccounts: number;
  totalAmount: number;
  metadata?: string;
  isActive: boolean;
  createdAt: string;
  createdBy?: { id: string; displayName: string };
  updatedAt?: string;
}

export interface CreatePortfolioRequest {
  externalId?: string;
  clientId: string;
  name: string;
  code: string;
  description?: string;
  receivedDate?: string;
  status?: PortfolioStatus;
}

export interface UpdatePortfolioRequest extends CreatePortfolioRequest {
  id: string;
}

export interface ChangeStatusRequest {
  portfolioId: string;
  status: PortfolioStatus;
}
