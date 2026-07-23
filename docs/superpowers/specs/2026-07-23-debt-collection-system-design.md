# Debt Collection Management System — Design Specification

**Date:** 2026-07-23  
**Status:** Draft  
**Sprint:** 1 — Foundation

---

## 1. Overview

A debt collection management software for managing debtors, debt accounts, collection cases, payments, and interactions. The system is single-tenant, CRUD-heavy with many forms, and requires comprehensive audit logging.

### 1.1 Goals

- Provide a robust platform for managing debt collection workflows
- Support multiple creditor clients and their debt portfolios
- Enable collection agents to work cases efficiently
- Maintain full audit trail for compliance
- Integrate with enterprise identity providers (Entra ID, LDAP, SAML, OIDC)

### 1.2 Non-Goals (Sprint 1)

- Debtor/debt account management (Sprint 2+)
- Collection case workflows (Sprint 2+)
- Payment processing (Sprint 2+)
- Reporting and analytics (Sprint 2+)
- Import/export functionality (Sprint 2+)

---

## 2. Technology Stack

### 2.1 Frontend

| Technology | Purpose |
|------------|---------|
| React 18+ | UI framework |
| TypeScript | Type safety |
| Vite | Build tool and dev server |
| TanStack Query | Server state management, caching |
| React Hook Form | Form state management |
| Zod | Schema validation |
| AG Grid | Enterprise data grid |
| shadcn/ui | UI component library |
| Tailwind CSS | Styling |
| Lucide Icons | Icon library |

**shadcn/ui Preset:** `buKo7Xs`
- Style: lyra
- Base Color: neutral
- Theme: sky
- Font: Inter
- Radius: default

### 2.2 Backend

| Technology | Purpose |
|------------|---------|
| ASP.NET Core 8+ | Web API framework |
| Entity Framework Core | ORM |
| FluentValidation | Request validation |
| MediatR | CQRS pattern |
| Mapster | Object mapping |

### 2.3 Database

| Technology | Purpose |
|------------|---------|
| PostgreSQL 16 | Primary database |
| Redis 7 | Caching, session storage |

### 2.4 Infrastructure

| Technology | Purpose |
|------------|---------|
| Docker Compose | Local development orchestration |
| Nginx | Reverse proxy |
| Keycloak 24 | Identity and access management |
| Hangfire | Background job processing (Sprint 2+) |
| MinIO | Object storage (Sprint 2+) |

### 2.5 Observability

| Technology | Purpose |
|------------|---------|
| OpenTelemetry | Distributed tracing, metrics |
| Grafana | Dashboards and visualization |
| Centralized logging | Log aggregation |

---

## 3. Architecture

### 3.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         Frontend                                 │
│   React + Vite + TypeScript + shadcn/ui + TanStack Query        │
│   React Hook Form + AG Grid                                      │
└─────────────────────────────────────┬───────────────────────────┘
                                      │ REST API (HTTPS)
┌─────────────────────────────────────┴───────────────────────────┐
│                        Nginx (Reverse Proxy)                     │
└─────────────────────────────────────┬───────────────────────────┘
                                      │
┌─────────────────────────────────────┴───────────────────────────┐
│                     ASP.NET Core Backend                         │
│  ┌────────────┐ ┌────────────┐ ┌────────────┐ ┌──────────────┐  │
│  │  Identity  │ │  Clients   │ │ Portfolios │ │    Audit     │  │
│  │   Module   │ │   Module   │ │   Module   │ │    Module    │  │
│  └────────────┘ └────────────┘ └────────────┘ └──────────────┘  │
│                                                                  │
│  ┌──────────────────────────────────────────────────────────┐   │
│  │                    Shared Kernel                          │   │
│  │   (Common entities, interfaces, base classes, utilities)  │   │
│  └──────────────────────────────────────────────────────────┘   │
└──────────────┬─────────────────┬────────────────┬───────────────┘
               │                 │                │
        ┌──────┴──────┐   ┌──────┴──────┐  ┌──────┴──────┐
        │ PostgreSQL  │   │    Redis    │  │  Keycloak   │
        │  (EF Core)  │   │   (Cache)   │  │   (Auth)    │
        └─────────────┘   └─────────────┘  └─────────────┘
```

### 3.2 Modular Monolith Structure

```
src/
├── Api/                              # ASP.NET Core Web API host
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Controllers/                  # Thin controllers
│
├── Modules/
│   ├── Identity/                     # Users, Roles, Authentication
│   │   ├── Identity.Core/            # Domain entities, interfaces
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   └── Role.cs
│   │   │   └── Interfaces/
│   │   │       └── IUserRepository.cs
│   │   ├── Identity.Application/     # Use cases, DTOs, validators
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   ├── DTOs/
│   │   │   └── Validators/
│   │   └── Identity.Infrastructure/  # EF, Keycloak integration
│   │       ├── Persistence/
│   │       └── Services/
│   │
│   ├── Clients/                      # Clients (creditors)
│   │   ├── Clients.Core/
│   │   ├── Clients.Application/
│   │   └── Clients.Infrastructure/
│   │
│   ├── Portfolios/                   # Portfolios
│   │   ├── Portfolios.Core/
│   │   ├── Portfolios.Application/
│   │   └── Portfolios.Infrastructure/
│   │
│   └── Audit/                        # Audit logging
│       ├── Audit.Core/
│       ├── Audit.Application/
│       └── Audit.Infrastructure/
│
├── Shared/
│   └── Shared.Kernel/                # Base entities, interfaces
│       ├── Entities/
│       │   ├── BaseEntity.cs
│       │   └── AuditableEntity.cs
│       ├── Interfaces/
│       │   ├── IRepository.cs
│       │   └── IAuditService.cs
│       └── Extensions/
│
└── frontend/                         # React application
    ├── src/
    │   ├── components/               # shadcn/ui components
    │   │   ├── ui/                   # Base UI components
    │   │   └── layout/               # Layout components
    │   │       ├── Sidebar.tsx
    │   │       ├── Header.tsx
    │   │       └── MainLayout.tsx
    │   ├── features/                 # Feature-based organization
    │   │   ├── auth/
    │   │   │   ├── components/
    │   │   │   ├── hooks/
    │   │   │   └── api/
    │   │   ├── users/
    │   │   ├── clients/
    │   │   ├── portfolios/
    │   │   └── audit/
    │   ├── hooks/                    # Shared custom hooks
    │   ├── lib/                      # Utilities, API client
    │   │   ├── api-client.ts
    │   │   ├── auth.ts
    │   │   └── utils.ts
    │   ├── types/                    # TypeScript types
    │   └── App.tsx
    ├── package.json
    ├── vite.config.ts
    ├── tailwind.config.js
    └── tsconfig.json
```

---

## 4. Data Model

### 4.1 Shared Base Classes

```csharp
// BaseEntity - all entities inherit from this
public abstract class BaseEntity
{
    public Guid Id { get; set; }
}

// AuditableEntity - entities with audit fields
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
}
```

### 4.2 Entity Definitions

#### Organization

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| name | varchar(255) | NOT NULL | Organization name |
| code | varchar(50) | UNIQUE, NOT NULL | Short code for reference |
| settings | jsonb | | Application settings |
| is_active | boolean | DEFAULT true | Active status |
| created_at | timestamp | NOT NULL | Creation timestamp |
| created_by | UUID | FK → users | Creator |
| updated_at | timestamp | | Last update timestamp |
| updated_by | UUID | FK → users | Last updater |

#### Role

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| name | varchar(100) | UNIQUE, NOT NULL | Role name |
| description | varchar(500) | | Role description |
| permissions | text[] | | Array of permission codes |
| is_system | boolean | DEFAULT false | System role flag (cannot delete) |
| created_at | timestamp | NOT NULL | Creation timestamp |
| updated_at | timestamp | | Last update timestamp |

**Default Roles:**

| Role | Permissions |
|------|-------------|
| SystemAdmin | `*` (all permissions) |
| Manager | `clients:read`, `clients:write`, `portfolios:read`, `portfolios:write`, `users:read`, `audit:read` |
| Agent | `clients:read`, `portfolios:read` |
| Viewer | `clients:read`, `portfolios:read` |

#### User

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| keycloak_id | varchar(255) | UNIQUE, NOT NULL | External ID from Keycloak |
| email | varchar(255) | UNIQUE, NOT NULL | Email address |
| first_name | varchar(100) | NOT NULL | First name |
| last_name | varchar(100) | NOT NULL | Last name |
| display_name | varchar(200) | | Display name |
| role_id | UUID | FK → roles, NOT NULL | User role |
| is_active | boolean | DEFAULT true | Active status |
| last_login_at | timestamp | | Last login timestamp |
| created_at | timestamp | NOT NULL | Creation timestamp |
| created_by | UUID | FK → users | Creator |
| updated_at | timestamp | | Last update timestamp |
| updated_by | UUID | FK → users | Last updater |

#### Client

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| external_id | varchar(255) | UNIQUE | External system ID for sync |
| name | varchar(255) | NOT NULL | Client name |
| code | varchar(50) | UNIQUE, NOT NULL | Short code (e.g., "BANK-ABC") |
| contact_name | varchar(200) | | Primary contact name |
| contact_email | varchar(255) | | Primary contact email |
| contact_phone | varchar(50) | | Primary contact phone |
| address | text | | Address |
| notes | text | | Internal notes |
| is_active | boolean | DEFAULT true | Active status |
| created_at | timestamp | NOT NULL | Creation timestamp |
| created_by | UUID | FK → users | Creator |
| updated_at | timestamp | | Last update timestamp |
| updated_by | UUID | FK → users | Last updater |

#### Portfolio

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| external_id | varchar(255) | UNIQUE | External system ID for sync |
| client_id | UUID | FK → clients, NOT NULL | Parent client |
| name | varchar(255) | NOT NULL | Portfolio name |
| code | varchar(50) | UNIQUE, NOT NULL | Short code (e.g., "BANK-ABC-2026-Q1") |
| description | text | | Description |
| received_date | date | | Date portfolio was received |
| status | varchar(50) | NOT NULL, DEFAULT 'Draft' | Status: Draft, Active, Closed, Archived |
| total_accounts | int | DEFAULT 0 | Denormalized account count |
| total_amount | decimal(18,2) | DEFAULT 0 | Denormalized total amount |
| metadata | jsonb | | Flexible additional data |
| is_active | boolean | DEFAULT true | Active status |
| created_at | timestamp | NOT NULL | Creation timestamp |
| created_by | UUID | FK → users | Creator |
| updated_at | timestamp | | Last update timestamp |
| updated_by | UUID | FK → users | Last updater |

**Portfolio Status Flow:**
```
Draft → Active → Closed → Archived
                    ↓
                 Active (reopen)
```

#### Audit Event

| Column | Type | Constraints | Description |
|--------|------|-------------|-------------|
| id | UUID | PK | Primary key |
| event_type | varchar(50) | NOT NULL | Event type |
| entity_type | varchar(100) | | Entity type name |
| entity_id | UUID | | Entity ID |
| user_id | UUID | FK → users | Acting user |
| user_email | varchar(255) | | Denormalized user email |
| ip_address | varchar(45) | | Client IP address |
| user_agent | varchar(500) | | Client user agent |
| old_values | jsonb | | Previous state |
| new_values | jsonb | | New state |
| metadata | jsonb | | Additional context |
| created_at | timestamp | NOT NULL | Event timestamp |

**Indexes:**
- `idx_audit_events_entity` on `(entity_type, entity_id)`
- `idx_audit_events_user` on `(user_id)`
- `idx_audit_events_type` on `(event_type)`
- `idx_audit_events_created` on `(created_at DESC)`

**Event Types:**
- Entity events: `Create`, `Update`, `Delete`
- Auth events: `Login`, `Logout`, `LoginFailed`
- Admin events: `RoleChange`, `UserActivate`, `UserDeactivate`
- Data events: `Export`, `Import`

---

## 5. API Design

### 5.1 Base URL

```
/api/v1
```

### 5.2 Authentication Endpoints

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/auth/login` | Redirect to Keycloak | No |
| POST | `/auth/logout` | End session | Yes |
| GET | `/auth/me` | Get current user info | Yes |
| POST | `/auth/refresh` | Refresh access token | Yes |

### 5.3 User Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/users` | List users (paginated) | SystemAdmin |
| GET | `/users/{id}` | Get user by ID | SystemAdmin |
| POST | `/users` | Create user | SystemAdmin |
| PUT | `/users/{id}` | Update user | SystemAdmin |
| DELETE | `/users/{id}` | Soft delete user | SystemAdmin |
| PUT | `/users/{id}/role` | Change user role | SystemAdmin |
| PUT | `/users/{id}/activate` | Activate user | SystemAdmin |
| PUT | `/users/{id}/deactivate` | Deactivate user | SystemAdmin |

### 5.4 Role Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/roles` | List all roles | SystemAdmin |
| GET | `/roles/{id}` | Get role details | SystemAdmin |

### 5.5 Client Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/clients` | List clients (paginated) | All |
| GET | `/clients/{id}` | Get client by ID | All |
| POST | `/clients` | Create client | SystemAdmin, Manager |
| PUT | `/clients/{id}` | Update client | SystemAdmin, Manager |
| DELETE | `/clients/{id}` | Soft delete client | SystemAdmin, Manager |
| GET | `/clients/{id}/portfolios` | List client portfolios | All |

### 5.6 Portfolio Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/portfolios` | List portfolios (paginated) | All |
| GET | `/portfolios/{id}` | Get portfolio by ID | All |
| POST | `/portfolios` | Create portfolio | SystemAdmin, Manager |
| PUT | `/portfolios/{id}` | Update portfolio | SystemAdmin, Manager |
| DELETE | `/portfolios/{id}` | Soft delete portfolio | SystemAdmin, Manager |
| PUT | `/portfolios/{id}/status` | Change portfolio status | SystemAdmin, Manager |

### 5.7 Audit Endpoints

| Method | Endpoint | Description | Roles |
|--------|----------|-------------|-------|
| GET | `/audit` | List audit events (paginated) | SystemAdmin, Manager |
| GET | `/audit/entity/{type}/{id}` | Get entity history | SystemAdmin, Manager |
| GET | `/audit/user/{id}` | Get user activity | SystemAdmin |

### 5.8 Request/Response Formats

#### Pagination Request

```
GET /api/v1/clients?page=1&pageSize=20&search=bank&sortBy=name&sortOrder=asc
```

#### Success Response (List)

```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Bank ABC",
      "code": "BANK-ABC",
      "isActive": true,
      "createdAt": "2026-07-23T10:00:00Z"
    }
  ],
  "meta": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

#### Success Response (Single)

```json
{
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "Bank ABC",
    "code": "BANK-ABC",
    "contactName": "John Doe",
    "contactEmail": "john@bankabc.com",
    "isActive": true,
    "createdAt": "2026-07-23T10:00:00Z",
    "createdBy": {
      "id": "...",
      "displayName": "Admin User"
    }
  }
}
```

#### Error Response

```json
{
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "Validation failed",
    "details": [
      {
        "field": "email",
        "message": "Email is required"
      },
      {
        "field": "code",
        "message": "Code must be unique"
      }
    ]
  }
}
```

#### Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| VALIDATION_ERROR | 400 | Request validation failed |
| UNAUTHORIZED | 401 | Not authenticated |
| FORBIDDEN | 403 | Not authorized |
| NOT_FOUND | 404 | Resource not found |
| CONFLICT | 409 | Duplicate resource |
| INTERNAL_ERROR | 500 | Server error |

---

## 6. Frontend Design

### 6.1 Layout

```
┌──────────────────────────────────────────────────────────────────┐
│  [≡]  Debt Collection System                          [User ▼]  │
├────────────┬─────────────────────────────────────────────────────┤
│            │                                                     │
│  ◇ Logo    │                                                     │
│            │                                                     │
│  Dashboard │                                                     │
│            │                    Page Content                     │
│  MAIN      │                                                     │
│  ▸ Clients │                                                     │
│  ▸ Portfolios                                                    │
│            │                                                     │
│  ADMIN     │                                                     │
│  ▸ Users   │                                                     │
│  ▸ Audit   │                                                     │
│            │                                                     │
│  [<<]      │                                                     │
└────────────┴─────────────────────────────────────────────────────┘
```

#### Sidebar Behavior

- **Expanded:** Icons + labels, grouped with section headers
- **Collapsed:** Icons only with tooltips on hover
- **Toggle:** Button at bottom `[<<]` / `[>>]`
- **Persistence:** Remember state in localStorage
- **Responsive:** Auto-collapse on screens < 1024px, overlay on mobile

### 6.2 Pages

| Route | Page | Description |
|-------|------|-------------|
| `/login` | Login | Keycloak redirect |
| `/` | Dashboard | Welcome, quick stats |
| `/clients` | Clients List | AG Grid with CRUD |
| `/clients/new` | Client Form | Create client |
| `/clients/:id` | Client Detail | View + portfolios list |
| `/clients/:id/edit` | Client Form | Edit client |
| `/portfolios` | Portfolios List | AG Grid with filters |
| `/portfolios/new` | Portfolio Form | Create portfolio |
| `/portfolios/:id` | Portfolio Detail | View portfolio |
| `/portfolios/:id/edit` | Portfolio Form | Edit portfolio |
| `/admin/users` | Users List | AG Grid (Admin only) |
| `/admin/users/new` | User Form | Create user |
| `/admin/users/:id/edit` | User Form | Edit user |
| `/admin/audit` | Audit Log | AG Grid with filters |

### 6.3 Role-Based Access

| Page | SystemAdmin | Manager | Agent | Viewer |
|------|:-----------:|:-------:|:-----:|:------:|
| Dashboard | ✓ | ✓ | ✓ | ✓ |
| Clients List | ✓ | ✓ | ✓ | ✓ |
| Client Create/Edit | ✓ | ✓ | ✗ | ✗ |
| Portfolios List | ✓ | ✓ | ✓ | ✓ |
| Portfolio Create/Edit | ✓ | ✓ | ✗ | ✗ |
| Users | ✓ | ✗ | ✗ | ✗ |
| Audit Log | ✓ | ✓ | ✗ | ✗ |

### 6.4 Component Library

Using shadcn/ui with preset `buKo7Xs`:

| Component | Usage |
|-----------|-------|
| Button | Actions, form submit |
| Input | Text inputs |
| Select | Dropdowns |
| Dialog | Modals for confirmations |
| Sheet | Side panels for forms |
| Form | Form wrapper with validation |
| Table | Simple tables (non-grid) |
| Card | Dashboard cards |
| Badge | Status indicators |
| Skeleton | Loading states |
| Toast | Notifications |

AG Grid for data-heavy lists (Clients, Portfolios, Users, Audit).

---

## 7. Authentication & Authorization

### 7.1 Keycloak Integration

```
┌──────────┐     ┌──────────┐     ┌──────────┐
│ Frontend │────▶│ Keycloak │────▶│ Identity │
│          │◀────│          │◀────│ Provider │
└──────────┘     └──────────┘     └──────────┘
      │                                  │
      │         ┌──────────┐             │
      └────────▶│   API    │◀────────────┘
                └──────────┘
```

**Supported Identity Providers:**
- Username/password (Keycloak local)
- Microsoft Entra ID (Azure AD)
- LDAP
- SAML 2.0
- Generic OIDC

### 7.2 Token Flow

1. User clicks Login → Redirect to Keycloak
2. Keycloak authenticates (direct or via IdP)
3. Keycloak returns JWT tokens (access + refresh)
4. Frontend stores tokens securely (httpOnly cookie preferred)
5. API validates JWT on each request
6. User info synced to local `users` table on first login

### 7.3 Permission Checking

```csharp
// Controller example
[Authorize(Policy = "clients:write")]
[HttpPost("clients")]
public async Task<IActionResult> CreateClient(CreateClientRequest request)
{
    // ...
}
```

---

## 8. Audit System

### 8.1 Automatic Change Tracking

Using EF Core interceptors to capture changes:

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(...)
    {
        var entries = context.ChangeTracker.Entries<AuditableEntity>();
        foreach (var entry in entries)
        {
            // Capture old/new values
            // Create AuditEvent
        }
    }
}
```

### 8.2 Sensitive Action Logging

Explicit logging for:
- Login/Logout/LoginFailed
- Role changes
- User activation/deactivation
- Data exports

```csharp
await _auditService.LogAsync(new AuditEvent
{
    EventType = "RoleChange",
    EntityType = "User",
    EntityId = userId,
    OldValues = new { RoleId = oldRoleId },
    NewValues = new { RoleId = newRoleId }
});
```

---

## 9. Infrastructure

### 9.1 Docker Compose

```yaml
services:
  frontend:
    build:
      context: ./frontend
      dockerfile: Dockerfile.dev
    ports:
      - "5173:5173"
    volumes:
      - ./frontend:/app
      - /app/node_modules
    environment:
      - VITE_API_URL=http://localhost:5000

  api:
    build:
      context: ./src
      dockerfile: Api/Dockerfile
    ports:
      - "5000:5000"
    depends_on:
      - postgres
      - redis
      - keycloak
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__Default=Host=postgres;Database=debtcollection;Username=app;Password=app_password
      - Redis__Connection=redis:6379
      - Keycloak__Authority=http://keycloak:8080/realms/debtcollection
      - Keycloak__ClientId=debt-collection-api

  postgres:
    image: postgres:16-alpine
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    environment:
      - POSTGRES_DB=debtcollection
      - POSTGRES_USER=app
      - POSTGRES_PASSWORD=app_password

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    ports:
      - "8080:8080"
    environment:
      - KEYCLOAK_ADMIN=admin
      - KEYCLOAK_ADMIN_PASSWORD=admin
      - KC_DB=postgres
      - KC_DB_URL=jdbc:postgresql://postgres:5432/keycloak
      - KC_DB_USERNAME=app
      - KC_DB_PASSWORD=app_password
    command: start-dev
    depends_on:
      - postgres

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"
    volumes:
      - ./nginx.conf:/etc/nginx/nginx.conf:ro
    depends_on:
      - api
      - frontend

volumes:
  postgres_data:
  redis_data:
```

### 9.2 Development Workflow

```bash
# Start all services
docker compose up -d

# View logs
docker compose logs -f api

# Run migrations
docker compose exec api dotnet ef database update

# Stop all services
docker compose down
```

---

## 10. Sprint 1 Deliverables

### 10.1 Backend

- [ ] Project structure (modular monolith)
- [ ] Shared kernel (base entities, interfaces)
- [ ] EF Core setup with PostgreSQL
- [ ] Database migrations for all Sprint 1 entities
- [ ] Keycloak integration
- [ ] JWT authentication middleware
- [ ] Role-based authorization policies
- [ ] Identity module (Users, Roles)
- [ ] Clients module (CRUD)
- [ ] Portfolios module (CRUD)
- [ ] Audit module (automatic tracking + explicit logging)
- [ ] Redis caching setup
- [ ] OpenTelemetry integration
- [ ] API documentation (Swagger)

### 10.2 Frontend

- [ ] Vite + React + TypeScript setup
- [ ] shadcn/ui with preset `buKo7Xs`
- [ ] TanStack Query setup
- [ ] React Hook Form + Zod setup
- [ ] AG Grid integration
- [ ] Collapsible sidebar layout
- [ ] Authentication flow (Keycloak)
- [ ] Route guards (role-based)
- [ ] Dashboard page (minimal)
- [ ] Clients list + CRUD forms
- [ ] Portfolios list + CRUD forms
- [ ] Users list + CRUD forms (Admin)
- [ ] Audit log viewer (Admin/Manager)
- [ ] Toast notifications
- [ ] Loading states
- [ ] Error handling

### 10.3 Infrastructure

- [ ] Docker Compose configuration
- [ ] Nginx reverse proxy config
- [ ] Keycloak realm setup
- [ ] Development environment documentation

---

## 11. Future Sprints (Out of Scope)

### Sprint 2 — Debt Management
- Debtors (CRUD, contacts, addresses)
- Debt accounts
- Collection cases
- Case assignments

### Sprint 3 — Collection Workflow
- Interactions
- Promises to pay
- Tasks
- Status histories

### Sprint 4 — Payments & Import
- Payments
- Payment allocations
- Import batches
- Import rows

### Sprint 5 — Advanced Features
- SignalR real-time updates
- Hangfire background jobs
- MinIO file storage
- Reporting
- Export functionality

---

## 12. Appendix

### A. Full Data Model (Future Sprints)

```
organizations
users
roles
teams
team_members

clients
portfolios
import_batches
import_rows

debtors
debtor_contacts
debtor_addresses

debt_accounts
collection_cases
case_assignments

interactions
interaction_outcomes

promises_to_pay
payments
payment_allocations

tasks
case_status_histories
audit_events
```

### B. Permission Codes

```
# Wildcard
*                    # All permissions (SystemAdmin)

# Clients
clients:read
clients:write
clients:delete

# Portfolios
portfolios:read
portfolios:write
portfolios:delete

# Users
users:read
users:write
users:delete

# Audit
audit:read
```
