# Sprint 2 — Import and Case Creation Design Specification

**Date:** 2026-07-23  
**Status:** Approved  
**Sprint:** 2 — Import and Case Creation

---

## 1. Overview

Sprint 2 extends the debt collection system with core debt management capabilities: Debtors, Debt Accounts, Collection Cases, and a CSV import workflow with preview and rollback functionality.

### 1.1 Goals

- Enable manual creation of Debtors (individuals and companies)
- Support bulk import of Debt Accounts via CSV with validation and preview
- Automatic Collection Case creation for each imported account
- Full import history with rollback capability
- CRUD operations for all new entities

### 1.2 Scope

| Feature | Included |
|---------|----------|
| Debtor CRUD (individual + company) | ✓ |
| Debtor contacts (multiple) | ✓ |
| Debtor addresses (multiple) | ✓ |
| Debt Account CRUD | ✓ |
| Collection Case (auto-created) | ✓ |
| CSV Import with preview | ✓ |
| Import validation (all-or-nothing) | ✓ |
| Import history & rollback | ✓ |
| Case assignment | ✓ |
| Excel import | ✗ (future) |
| Payment tracking | ✗ (Sprint 3+) |
| Interactions/notes | ✗ (Sprint 3+) |

---

## 2. Data Model

### 2.1 Debtor

Represents a person or company who owes money.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| external_id | varchar(255) | UNIQUE, NOT NULL | ID number for matching |
| debtor_type | varchar(20) | NOT NULL | "individual" or "company" |
| first_name | varchar(100) | | For individuals |
| last_name | varchar(100) | | For individuals |
| company_name | varchar(255) | | For companies |
| display_name | varchar(255) | NOT NULL | Computed display name |
| date_of_birth | date | | For individuals |
| tax_id | varchar(50) | | Company tax ID |
| notes | text | | Free-form notes |
| is_active | boolean | DEFAULT true | Active status |
| created_at | timestamp | NOT NULL | |
| created_by | UUID | FK → users | |
| updated_at | timestamp | | |
| updated_by | UUID | FK → users | |

**Indexes:**
- `idx_debtors_external_id` on `(external_id)` UNIQUE
- `idx_debtors_display_name` on `(display_name)`
- `idx_debtors_type` on `(debtor_type)`

**Display Name Logic:**
- Individual: `first_name + " " + last_name`
- Company: `company_name`

### 2.2 DebtorContact

Multiple contacts per debtor.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| debtor_id | UUID | FK → debtors, NOT NULL | Parent debtor |
| type | varchar(50) | NOT NULL | "phone" or "email" |
| label | varchar(50) | | "home", "work", "mobile", "personal" |
| value | varchar(255) | NOT NULL | Phone number or email |
| is_primary | boolean | DEFAULT false | Primary contact flag |
| created_at | timestamp | NOT NULL | |
| updated_at | timestamp | | |

**Indexes:**
- `idx_debtor_contacts_debtor` on `(debtor_id)`

### 2.3 DebtorAddress

Multiple addresses per debtor.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| debtor_id | UUID | FK → debtors, NOT NULL | Parent debtor |
| label | varchar(50) | | "home", "work", "mailing" |
| address_line1 | varchar(255) | NOT NULL | Street address |
| address_line2 | varchar(255) | | Unit/apt |
| city | varchar(100) | | City |
| state | varchar(100) | | State/province |
| postal_code | varchar(20) | | Zip/postal code |
| country | varchar(100) | | Country |
| is_primary | boolean | DEFAULT false | Primary address flag |
| created_at | timestamp | NOT NULL | |
| updated_at | timestamp | | |

**Indexes:**
- `idx_debtor_addresses_debtor` on `(debtor_id)`

### 2.4 DebtAccount

A specific debt owed by a debtor.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| external_id | varchar(255) | | External reference |
| debtor_id | UUID | FK → debtors, NOT NULL | Owning debtor |
| portfolio_id | UUID | FK → portfolios, NOT NULL | Parent portfolio |
| import_batch_id | UUID | FK → import_batches | Source import (nullable) |
| account_number | varchar(100) | UNIQUE, NOT NULL | Account number |
| creditor_reference | varchar(255) | | Original creditor ref |
| original_amount | decimal(18,2) | NOT NULL | Original debt amount |
| current_balance | decimal(18,2) | NOT NULL | Current balance |
| interest_amount | decimal(18,2) | DEFAULT 0 | Accrued interest |
| fees_amount | decimal(18,2) | DEFAULT 0 | Fees |
| due_date | date | | Original due date |
| days_past_due | int | DEFAULT 0 | Days overdue |
| last_payment_date | date | | Last payment received |
| last_payment_amount | decimal(18,2) | | Last payment amount |
| status | varchar(50) | DEFAULT 'Open' | Open, Closed, Paid, WrittenOff |
| notes | text | | Free-form notes |
| is_active | boolean | DEFAULT true | Active status |
| created_at | timestamp | NOT NULL | |
| created_by | UUID | FK → users | |
| updated_at | timestamp | | |
| updated_by | UUID | FK → users | |

**Indexes:**
- `idx_debt_accounts_debtor` on `(debtor_id)`
- `idx_debt_accounts_portfolio` on `(portfolio_id)`
- `idx_debt_accounts_import` on `(import_batch_id)`
- `idx_debt_accounts_account_number` on `(account_number)` UNIQUE
- `idx_debt_accounts_status` on `(status)`

**Account Status Values:**
- `Open` - Active, being collected
- `Closed` - Closed, no longer active
- `Paid` - Fully paid
- `WrittenOff` - Written off as uncollectable

### 2.5 CollectionCase

Work item for collecting on a debt account.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| case_number | varchar(50) | UNIQUE, NOT NULL | Auto-generated case # |
| debt_account_id | UUID | FK → debt_accounts, UNIQUE, NOT NULL | Linked account |
| assigned_to | UUID | FK → users | Assigned agent (nullable) |
| status | varchar(50) | DEFAULT 'New' | Case status |
| priority | varchar(20) | DEFAULT 'Medium' | Low, Medium, High |
| opened_at | timestamp | NOT NULL | When case was created |
| closed_at | timestamp | | When case was closed |
| notes | text | | Case notes |
| created_at | timestamp | NOT NULL | |
| created_by | UUID | FK → users | |
| updated_at | timestamp | | |
| updated_by | UUID | FK → users | |

**Indexes:**
- `idx_cases_debt_account` on `(debt_account_id)` UNIQUE
- `idx_cases_assigned_to` on `(assigned_to)`
- `idx_cases_status` on `(status)`
- `idx_cases_case_number` on `(case_number)` UNIQUE

**Case Status Values:**
- `New` - Newly created, not yet worked
- `InProgress` - Being actively worked
- `Pending` - Waiting for response/action
- `Closed` - Completed (paid, settled, etc.)
- `Cancelled` - Cancelled/invalid

**Case Number Format:** `CASE-YYYYMMDD-XXXXX` (auto-generated)

### 2.6 ImportBatch

Tracks import history for audit and rollback.

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| portfolio_id | UUID | FK → portfolios, NOT NULL | Target portfolio |
| filename | varchar(255) | NOT NULL | Original filename |
| file_size | int | | File size in bytes |
| total_rows | int | NOT NULL | Total rows in file |
| created_debtors | int | DEFAULT 0 | New debtors created |
| matched_debtors | int | DEFAULT 0 | Existing debtors matched |
| created_accounts | int | DEFAULT 0 | Accounts created |
| created_cases | int | DEFAULT 0 | Cases created |
| status | varchar(50) | DEFAULT 'Completed' | Completed, RolledBack |
| error_message | text | | Error details if failed |
| rolled_back_at | timestamp | | When rollback occurred |
| rolled_back_by | UUID | FK → users | Who rolled back |
| created_at | timestamp | NOT NULL | |
| created_by | UUID | FK → users | |

**Indexes:**
- `idx_import_batches_portfolio` on `(portfolio_id)`
- `idx_import_batches_status` on `(status)`
- `idx_import_batches_created` on `(created_at DESC)`

---

## 3. API Design

### 3.1 Debtor Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/v1/debtors` | List debtors (paginated) | All |
| GET | `/api/v1/debtors/{id}` | Get debtor with contacts & addresses | All |
| POST | `/api/v1/debtors` | Create debtor | Manager+ |
| PUT | `/api/v1/debtors/{id}` | Update debtor | Manager+ |
| DELETE | `/api/v1/debtors/{id}` | Soft delete debtor | Manager+ |
| GET | `/api/v1/debtors/{id}/accounts` | List debtor's accounts | All |
| POST | `/api/v1/debtors/{id}/contacts` | Add contact | Manager+ |
| PUT | `/api/v1/debtors/{id}/contacts/{contactId}` | Update contact | Manager+ |
| DELETE | `/api/v1/debtors/{id}/contacts/{contactId}` | Remove contact | Manager+ |
| POST | `/api/v1/debtors/{id}/addresses` | Add address | Manager+ |
| PUT | `/api/v1/debtors/{id}/addresses/{addressId}` | Update address | Manager+ |
| DELETE | `/api/v1/debtors/{id}/addresses/{addressId}` | Remove address | Manager+ |

### 3.2 Debt Account Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/v1/accounts` | List accounts (paginated) | All |
| GET | `/api/v1/accounts/{id}` | Get account details | All |
| POST | `/api/v1/accounts` | Create account manually | Manager+ |
| PUT | `/api/v1/accounts/{id}` | Update account | Manager+ |
| DELETE | `/api/v1/accounts/{id}` | Soft delete account | Manager+ |
| GET | `/api/v1/accounts/{id}/case` | Get linked case | All |

### 3.3 Collection Case Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/api/v1/cases` | List cases (paginated) | All |
| GET | `/api/v1/cases/{id}` | Get case details | All |
| PUT | `/api/v1/cases/{id}` | Update case | Agent+ |
| PUT | `/api/v1/cases/{id}/assign` | Assign to agent | Manager+ |
| PUT | `/api/v1/cases/{id}/unassign` | Unassign case | Manager+ |
| GET | `/api/v1/cases/my` | Get my assigned cases | Agent+ |

### 3.4 Import Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| POST | `/api/v1/imports/validate` | Validate CSV, return preview | Manager+ |
| POST | `/api/v1/imports/confirm` | Confirm import | Manager+ |
| GET | `/api/v1/imports` | List import history | Manager+ |
| GET | `/api/v1/imports/{id}` | Get import details | Manager+ |
| POST | `/api/v1/imports/{id}/rollback` | Rollback import | SystemAdmin |
| GET | `/api/v1/imports/template` | Download CSV template | Manager+ |

### 3.5 Import API Details

#### Validate Endpoint

```
POST /api/v1/imports/validate
Content-Type: multipart/form-data

Fields:
  - portfolioId: UUID (required)
  - file: CSV file (required)

Response 200:
{
  "data": {
    "sessionId": "uuid",
    "rows": [
      {
        "rowNumber": 1,
        "debtorExternalId": "123456",
        "debtorType": "individual",
        "firstName": "John",
        "lastName": "Smith",
        "companyName": null,
        "phone": "081-234-5678",
        "email": "john@email.com",
        "address": "123 Main St",
        "debtorExists": false,
        "existingDebtorId": null,
        "accountNumber": "ACC-001",
        "originalAmount": 50000,
        "currentBalance": 65000,
        "interest": 10000,
        "fees": 5000,
        "dueDate": "2025-01-15",
        "daysPastDue": 180,
        "creditorReference": "BANK-REF-001",
        "warnings": [],
        "errors": []
      }
    ],
    "summary": {
      "totalRows": 100,
      "validRows": 100,
      "newDebtors": 80,
      "existingDebtors": 20,
      "totalOriginalAmount": 5000000,
      "totalCurrentBalance": 6500000
    },
    "isValid": true
  }
}

Response 400 (validation errors):
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "CSV validation failed",
    "details": [
      { "row": 5, "field": "original_amount", "message": "Must be a positive number" },
      { "row": 12, "field": "debtor_external_id", "message": "Required field missing" }
    ]
  }
}
```

#### Confirm Endpoint

```
POST /api/v1/imports/confirm
Content-Type: application/json

{
  "sessionId": "uuid",
  "portfolioId": "uuid",
  "rows": [
    {
      "rowNumber": 1,
      "debtorExternalId": "123456",
      "debtorType": "individual",
      "firstName": "John",
      "lastName": "Smith",
      "companyName": null,
      "phone": "081-234-5678",
      "email": "john@email.com",
      "address": "123 Main St, Bangkok",
      "accountNumber": "ACC-001",
      "originalAmount": 50000,
      "currentBalance": 65000,
      "interest": 10000,
      "fees": 5000,
      "dueDate": "2025-01-15",
      "daysPastDue": 180,
      "creditorReference": "BANK-REF-001"
    }
  ]
}

Response 201:
{
  "data": {
    "importBatchId": "uuid",
    "createdDebtors": 80,
    "matchedDebtors": 20,
    "createdAccounts": 100,
    "createdCases": 100
  }
}
```

#### Rollback Endpoint

```
POST /api/v1/imports/{id}/rollback

Response 200:
{
  "data": {
    "deletedAccounts": 100,
    "deletedCases": 100,
    "deletedDebtors": 80,
    "keptDebtors": 20
  }
}
```

Note: Rollback deletes accounts and cases from this import. Debtors are only deleted if they have no other accounts.

---

## 4. CSV Template

### 4.1 Column Definitions

| Column | Required | Type | Validation |
|--------|----------|------|------------|
| `debtor_external_id` | Yes | string | Not empty, used for matching |
| `debtor_type` | Yes | string | Must be "individual" or "company" |
| `first_name` | Conditional | string | Required if type=individual |
| `last_name` | Conditional | string | Required if type=individual |
| `company_name` | Conditional | string | Required if type=company |
| `phone` | No | string | Primary phone |
| `email` | No | string | Valid email format |
| `address` | No | string | Single-line address |
| `account_number` | Yes | string | Unique, not empty |
| `original_amount` | Yes | decimal | Positive number |
| `current_balance` | Yes | decimal | Positive number |
| `interest` | No | decimal | Non-negative number |
| `fees` | No | decimal | Non-negative number |
| `due_date` | No | date | YYYY-MM-DD format |
| `days_past_due` | No | integer | Non-negative integer |
| `creditor_reference` | No | string | External reference |

### 4.2 Example CSV

```csv
debtor_external_id,debtor_type,first_name,last_name,company_name,phone,email,address,account_number,original_amount,current_balance,interest,fees,due_date,days_past_due,creditor_reference
1234567890,individual,John,Smith,,081-234-5678,john@email.com,"123 Main St, Bangkok",ACC-001,50000,65000,10000,5000,2025-01-15,180,BANK-REF-001
9876543210,individual,Jane,Doe,,089-876-5432,jane@email.com,"456 Second Ave, Chiang Mai",ACC-002,30000,35000,3000,2000,2025-03-20,120,BANK-REF-002
COMP-001,company,,,ABC Corporation,02-123-4567,billing@abc.co.th,"789 Business Park, Bangkok",ACC-003,100000,120000,15000,5000,2025-02-01,150,BANK-REF-003
```

### 4.3 Validation Rules

| Rule | Error Message |
|------|---------------|
| Empty `debtor_external_id` | "Row {n}: Debtor ID is required" |
| Invalid `debtor_type` | "Row {n}: Debtor type must be 'individual' or 'company'" |
| Individual missing `first_name` | "Row {n}: First name required for individual debtor" |
| Individual missing `last_name` | "Row {n}: Last name required for individual debtor" |
| Company missing `company_name` | "Row {n}: Company name required for company debtor" |
| Empty `account_number` | "Row {n}: Account number is required" |
| Duplicate `account_number` in file | "Row {n}: Duplicate account number '{x}' in file" |
| `account_number` exists in database | "Row {n}: Account number '{x}' already exists" |
| Invalid `original_amount` | "Row {n}: Original amount must be a positive number" |
| Invalid `current_balance` | "Row {n}: Current balance must be a positive number" |
| Invalid `due_date` format | "Row {n}: Due date must be in YYYY-MM-DD format" |
| Invalid `email` format | "Row {n}: Invalid email format" |

---

## 5. Frontend Design

### 5.1 New Routes

| Route | Page | Description |
|-------|------|-------------|
| `/debtors` | Debtors List | AG Grid with search, type filter |
| `/debtors/new` | Debtor Form | Create debtor manually |
| `/debtors/{id}` | Debtor Detail | View debtor + contacts + addresses + accounts |
| `/debtors/{id}/edit` | Debtor Form | Edit debtor |
| `/accounts` | Accounts List | AG Grid with filters |
| `/accounts/new` | Account Form | Create account manually |
| `/accounts/{id}` | Account Detail | View account + case |
| `/accounts/{id}/edit` | Account Form | Edit account |
| `/cases` | Cases List | AG Grid (all cases) |
| `/cases/my` | My Cases | Agent's assigned cases |
| `/cases/{id}` | Case Detail | View/work case |
| `/imports` | Import History | List past imports |
| `/imports/new` | Import Wizard | 3-step import flow |

### 5.2 Navigation Update

```
MAIN
  Dashboard
  Clients
  Portfolios
  Debtors        ← NEW
  Accounts       ← NEW
  Cases          ← NEW

WORK
  My Cases       ← NEW (visible to Agents)

ADMIN
  Imports        ← NEW
  Users
  Audit
```

### 5.3 Import Wizard (3 Steps)

**Step 1: Upload**
- Portfolio dropdown (required)
- Drag-and-drop file upload
- Download template link
- File validation feedback

**Step 2: Preview & Edit**
- Summary: total rows, new debtors, existing debtors
- AG Grid with all parsed data
- Inline editing capability
- Warning indicators for existing debtors
- Row delete action
- Validation status per row

**Step 3: Complete**
- Success message with counts
- Links to view import details or start new import

### 5.4 Role-Based Access

| Page | SystemAdmin | Manager | Agent | Viewer |
|------|:-----------:|:-------:|:-----:|:------:|
| Debtors List | ✓ | ✓ | ✓ | ✓ |
| Debtor Create/Edit | ✓ | ✓ | ✗ | ✗ |
| Accounts List | ✓ | ✓ | ✓ | ✓ |
| Account Create/Edit | ✓ | ✓ | ✗ | ✗ |
| Cases List | ✓ | ✓ | ✓ | ✓ |
| Case Update | ✓ | ✓ | ✓ | ✗ |
| Case Assign | ✓ | ✓ | ✗ | ✗ |
| My Cases | ✓ | ✓ | ✓ | ✗ |
| Import Wizard | ✓ | ✓ | ✗ | ✗ |
| Import History | ✓ | ✓ | ✗ | ✗ |
| Import Rollback | ✓ | ✗ | ✗ | ✗ |

---

## 6. Import Processing Flow

### 6.1 Synchronous Processing

Import uses synchronous processing for immediate feedback:

1. **Upload & Parse** - Read CSV, parse all rows
2. **Validate All** - Check all validation rules
3. **Return Preview** - If valid, return parsed data; if invalid, return errors
4. **User Edits** - User reviews/edits in preview grid
5. **Confirm** - User submits edited data
6. **Process in Transaction**:
   - Create ImportBatch record
   - For each row:
     - Find or create Debtor (match by external_id)
     - Create primary Contact if phone/email provided
     - Create primary Address if address provided
     - Create DebtAccount (linked to ImportBatch)
     - Create CollectionCase
   - Update ImportBatch with counts
7. **Return Summary** - Success with created counts

### 6.2 Rollback Process

1. Load ImportBatch record
2. Find all DebtAccounts with this import_batch_id
3. For each account:
   - Delete linked CollectionCase
   - Delete DebtAccount
4. For each Debtor created by this import:
   - Check if debtor has other accounts
   - If no other accounts, delete Debtor (and contacts/addresses)
5. Update ImportBatch status to "RolledBack"
6. Record rolled_back_at and rolled_back_by

### 6.3 Debtor Matching Logic

```
For each row:
  1. Find Debtor by external_id
  2. If found:
     - Mark as "existing" in preview
     - Link new account to existing debtor
  3. If not found:
     - Mark as "new" in preview
     - Create new Debtor on confirm
```

---

## 7. Module Structure

Following the existing modular monolith pattern:

```
src/Modules/
├── Debtors/
│   └── Debtors.Core/
│       ├── Entities/
│       │   ├── Debtor.cs
│       │   ├── DebtorContact.cs
│       │   └── DebtorAddress.cs
│       ├── Enums/
│       │   └── DebtorType.cs
│       └── Interfaces/
│           └── IDebtorRepository.cs
│
├── Accounts/
│   └── Accounts.Core/
│       ├── Entities/
│       │   └── DebtAccount.cs
│       ├── Enums/
│       │   └── AccountStatus.cs
│       └── Interfaces/
│           └── IDebtAccountRepository.cs
│
├── Cases/
│   └── Cases.Core/
│       ├── Entities/
│       │   └── CollectionCase.cs
│       ├── Enums/
│       │   ├── CaseStatus.cs
│       │   └── CasePriority.cs
│       └── Interfaces/
│           └── ICaseRepository.cs
│
└── Imports/
    └── Imports.Core/
        ├── Entities/
        │   └── ImportBatch.cs
        ├── Enums/
        │   └── ImportStatus.cs
        ├── Interfaces/
        │   ├── IImportBatchRepository.cs
        │   └── IImportService.cs
        └── Services/
            ├── CsvParser.cs
            └── ImportValidator.cs
```

---

## 8. Sprint 2 Deliverables

### 8.1 Backend

- [ ] Debtors module (entity, repository, controller)
- [ ] DebtorContact entity and endpoints
- [ ] DebtorAddress entity and endpoints
- [ ] Accounts module (entity, repository, controller)
- [ ] Cases module (entity, repository, controller)
- [ ] Imports module (entity, repository, service, controller)
- [ ] CSV parser service
- [ ] Import validation service
- [ ] Rollback service
- [ ] Database migrations
- [ ] Case number auto-generation

### 8.2 Frontend

- [ ] Debtors feature (list, detail, form)
- [ ] Accounts feature (list, detail, form)
- [ ] Cases feature (list, detail, my cases)
- [ ] Import wizard (3-step flow)
- [ ] Import history page
- [ ] CSV template download
- [ ] Navigation updates
- [ ] Route protection by role

### 8.3 Testing

- [ ] Debtor API integration tests
- [ ] Account API integration tests
- [ ] Case API integration tests
- [ ] Import validation tests
- [ ] Import/rollback integration tests

---

## 9. Future Considerations (Out of Scope)

- Excel file support
- Background processing for large imports
- Import scheduling
- Duplicate detection beyond external_id
- Debtor merge functionality
- Case reassignment rules
- Automated case prioritization
