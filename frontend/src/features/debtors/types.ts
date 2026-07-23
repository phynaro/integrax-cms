export type DebtorType = 'Individual' | 'Company';
export type ContactType = 'Phone' | 'Email';
export type AddressLabel = 'Home' | 'Work' | 'Mailing' | 'Other';

export interface DebtorContact {
  id: string;
  type: ContactType;
  label?: string;
  value: string;
  isPrimary: boolean;
}

export interface DebtorAddress {
  id: string;
  label: AddressLabel;
  addressLine1: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isPrimary: boolean;
}

export interface Debtor {
  id: string;
  externalId: string;
  debtorType: DebtorType;
  firstName?: string;
  lastName?: string;
  companyName?: string;
  displayName: string;
  dateOfBirth?: string;
  taxId?: string;
  notes?: string;
  isActive: boolean;
  createdAt: string;
  contacts: DebtorContact[];
  addresses: DebtorAddress[];
}

export interface DebtorListItem {
  id: string;
  externalId: string;
  debtorType: DebtorType;
  displayName: string;
  isActive: boolean;
  createdAt: string;
  contactCount: number;
  accountCount: number;
}

export interface CreateDebtorRequest {
  externalId: string;
  debtorType: DebtorType;
  firstName?: string;
  lastName?: string;
  companyName?: string;
  dateOfBirth?: string;
  taxId?: string;
  notes?: string;
}

export interface UpdateDebtorRequest extends CreateDebtorRequest {
  id: string;
}

export interface CreateContactRequest {
  type: ContactType;
  label?: string;
  value: string;
  isPrimary: boolean;
}

export interface CreateAddressRequest {
  label: AddressLabel;
  addressLine1: string;
  addressLine2?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
  isPrimary: boolean;
}
