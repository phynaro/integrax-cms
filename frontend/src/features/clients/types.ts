export interface Client {
  id: string;
  externalId?: string;
  name: string;
  code: string;
  contactName?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  createdBy?: { id: string; displayName: string };
  updatedAt?: string;
}

export interface CreateClientRequest {
  externalId?: string;
  name: string;
  code: string;
  contactName?: string;
  contactEmail?: string;
  contactPhone?: string;
  address?: string;
  notes?: string;
}

export interface UpdateClientRequest extends CreateClientRequest {
  id: string;
}
