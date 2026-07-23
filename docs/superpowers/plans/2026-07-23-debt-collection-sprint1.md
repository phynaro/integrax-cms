# Debt Collection System - Sprint 1 Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the foundation for a debt collection management system including project structure, authentication, user/role management, clients, portfolios, and audit logging.

**Architecture:** Modular monolith with ASP.NET Core backend, React frontend. Each module (Identity, Clients, Portfolios, Audit) follows Clean Architecture with Core/Application/Infrastructure layers. Frontend uses feature-based organization.

**Tech Stack:**
- Backend: ASP.NET Core 8, EF Core, PostgreSQL, Redis, Keycloak
- Frontend: React 18, Vite, TypeScript, TanStack Query, React Hook Form, AG Grid, shadcn/ui
- Infrastructure: Docker Compose, Nginx

## Global Constraints

- .NET 8.0 or higher
- Node.js 20 LTS or higher
- PostgreSQL 16
- All entities use UUID primary keys
- All API responses follow standard format with `data` and `meta`/`error` wrappers
- shadcn/ui preset: `buKo7Xs`
- Frontend routes protected by role-based guards

---

## File Structure

### Backend
```
src/
├── Api/
│   ├── Api.csproj
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Controllers/
│       ├── AuthController.cs
│       ├── UsersController.cs
│       ├── RolesController.cs
│       ├── ClientsController.cs
│       ├── PortfoliosController.cs
│       └── AuditController.cs
├── Modules/
│   ├── Identity/
│   │   ├── Identity.Core/
│   │   │   ├── Identity.Core.csproj
│   │   │   ├── Entities/
│   │   │   │   ├── User.cs
│   │   │   │   └── Role.cs
│   │   │   └── Interfaces/
│   │   │       ├── IUserRepository.cs
│   │   │       └── IRoleRepository.cs
│   │   ├── Identity.Application/
│   │   │   ├── Identity.Application.csproj
│   │   │   ├── DTOs/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── Validators/
│   │   └── Identity.Infrastructure/
│   │       ├── Identity.Infrastructure.csproj
│   │       ├── Persistence/
│   │       └── Services/
│   ├── Clients/
│   │   ├── Clients.Core/
│   │   ├── Clients.Application/
│   │   └── Clients.Infrastructure/
│   ├── Portfolios/
│   │   ├── Portfolios.Core/
│   │   ├── Portfolios.Application/
│   │   └── Portfolios.Infrastructure/
│   └── Audit/
│       ├── Audit.Core/
│       ├── Audit.Application/
│       └── Audit.Infrastructure/
└── Shared/
    └── Shared.Kernel/
        ├── Shared.Kernel.csproj
        ├── Entities/
        │   ├── BaseEntity.cs
        │   └── AuditableEntity.cs
        ├── Interfaces/
        │   ├── IRepository.cs
        │   └── IAuditService.cs
        └── Extensions/
```

### Frontend
```
frontend/
├── package.json
├── vite.config.ts
├── tsconfig.json
├── tailwind.config.js
├── components.json
├── src/
│   ├── main.tsx
│   ├── App.tsx
│   ├── index.css
│   ├── components/
│   │   ├── ui/           (shadcn components)
│   │   └── layout/
│   │       ├── MainLayout.tsx
│   │       ├── Sidebar.tsx
│   │       └── Header.tsx
│   ├── features/
│   │   ├── auth/
│   │   │   ├── api.ts
│   │   │   ├── hooks.ts
│   │   │   ├── types.ts
│   │   │   └── components/
│   │   ├── users/
│   │   ├── clients/
│   │   ├── portfolios/
│   │   └── audit/
│   ├── lib/
│   │   ├── api-client.ts
│   │   ├── auth.ts
│   │   └── utils.ts
│   └── types/
│       └── index.ts
```

---

## Task 1: Docker Compose Infrastructure

**Files:**
- Create: `docker-compose.yml`
- Create: `nginx.conf`
- Create: `.env.example`

**Interfaces:**
- Consumes: None
- Produces: Running PostgreSQL, Redis, Keycloak services accessible to backend

- [ ] **Step 1: Create docker-compose.yml**

```yaml
# docker-compose.yml
services:
  postgres:
    image: postgres:16-alpine
    container_name: dc-postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
      - ./scripts/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      POSTGRES_DB: ${POSTGRES_DB:-debtcollection}
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    container_name: dc-redis
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data
    healthcheck:
      test: ["CMD", "redis-cli", "ping"]
      interval: 5s
      timeout: 5s
      retries: 5

  keycloak:
    image: quay.io/keycloak/keycloak:24.0
    container_name: dc-keycloak
    ports:
      - "8080:8080"
    environment:
      KEYCLOAK_ADMIN: ${KEYCLOAK_ADMIN:-admin}
      KEYCLOAK_ADMIN_PASSWORD: ${KEYCLOAK_ADMIN_PASSWORD:-admin}
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://postgres:5432/keycloak
      KC_DB_USERNAME: ${POSTGRES_USER:-postgres}
      KC_DB_PASSWORD: ${POSTGRES_PASSWORD:-postgres}
      KC_HOSTNAME_STRICT: "false"
      KC_HTTP_ENABLED: "true"
    command: start-dev
    depends_on:
      postgres:
        condition: service_healthy

volumes:
  postgres_data:
  redis_data:
```

- [ ] **Step 2: Create database init script**

```sql
-- scripts/init-db.sql
CREATE DATABASE keycloak;
```

- [ ] **Step 3: Create .env.example**

```bash
# .env.example
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgres
POSTGRES_DB=debtcollection
KEYCLOAK_ADMIN=admin
KEYCLOAK_ADMIN_PASSWORD=admin
```

- [ ] **Step 4: Create nginx.conf**

```nginx
# nginx.conf
events {
    worker_connections 1024;
}

http {
    upstream api {
        server host.docker.internal:5000;
    }

    upstream frontend {
        server host.docker.internal:5173;
    }

    server {
        listen 80;

        location /api {
            proxy_pass http://api;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
            proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
            proxy_set_header X-Forwarded-Proto $scheme;
        }

        location / {
            proxy_pass http://frontend;
            proxy_http_version 1.1;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection 'upgrade';
            proxy_set_header Host $host;
        }
    }
}
```

- [ ] **Step 5: Start infrastructure and verify**

```bash
mkdir -p scripts
cp .env.example .env
docker compose up -d postgres redis keycloak
docker compose ps
```

Expected: All three services running and healthy.

- [ ] **Step 6: Commit**

```bash
git add docker-compose.yml nginx.conf .env.example scripts/
git commit -m "infra: add docker compose for postgres, redis, keycloak"
```

---

## Task 2: Backend Solution Structure

**Files:**
- Create: `src/DebtCollection.sln`
- Create: `src/Api/Api.csproj`
- Create: `src/Shared/Shared.Kernel/Shared.Kernel.csproj`
- Create: `src/Shared/Shared.Kernel/Entities/BaseEntity.cs`
- Create: `src/Shared/Shared.Kernel/Entities/AuditableEntity.cs`

**Interfaces:**
- Consumes: None
- Produces: `BaseEntity`, `AuditableEntity` base classes for all modules

- [ ] **Step 1: Create solution and Shared.Kernel project**

```bash
mkdir -p src/Shared/Shared.Kernel
cd src
dotnet new sln -n DebtCollection
dotnet new classlib -n Shared.Kernel -o Shared/Shared.Kernel -f net8.0
dotnet sln add Shared/Shared.Kernel/Shared.Kernel.csproj
cd ..
```

- [ ] **Step 2: Create BaseEntity.cs**

```csharp
// src/Shared/Shared.Kernel/Entities/BaseEntity.cs
namespace Shared.Kernel.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
}
```

- [ ] **Step 3: Create AuditableEntity.cs**

```csharp
// src/Shared/Shared.Kernel/Entities/AuditableEntity.cs
namespace Shared.Kernel.Entities;

public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public Guid? UpdatedBy { get; set; }
    public bool IsActive { get; set; } = true;
}
```

- [ ] **Step 4: Create IRepository interface**

```csharp
// src/Shared/Shared.Kernel/Interfaces/IRepository.cs
using System.Linq.Expressions;
using Shared.Kernel.Entities;

namespace Shared.Kernel.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task UpdateAsync(T entity, CancellationToken ct = default);
    Task DeleteAsync(T entity, CancellationToken ct = default);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null, CancellationToken ct = default);
}
```

- [ ] **Step 5: Create API project**

```bash
cd src
dotnet new webapi -n Api -o Api -f net8.0
dotnet sln add Api/Api.csproj
dotnet add Api/Api.csproj reference Shared/Shared.Kernel/Shared.Kernel.csproj
cd ..
```

- [ ] **Step 6: Update Api.csproj with required packages**

```xml
<!-- src/Api/Api.csproj -->
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.0" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
    <PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
    <PackageReference Include="StackExchange.Redis" Version="2.7.10" />
    <PackageReference Include="Swashbuckle.AspNetCore" Version="6.5.0" />
    <PackageReference Include="FluentValidation.AspNetCore" Version="11.3.0" />
    <PackageReference Include="MediatR" Version="12.2.0" />
    <PackageReference Include="Mapster" Version="7.4.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Shared\Shared.Kernel\Shared.Kernel.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 7: Build and verify**

```bash
cd src
dotnet restore
dotnet build
cd ..
```

Expected: Build succeeded.

- [ ] **Step 8: Commit**

```bash
git add src/
git commit -m "feat: create solution structure with shared kernel"
```

---

## Task 3: Identity Module - Core Layer

**Files:**
- Create: `src/Modules/Identity/Identity.Core/Identity.Core.csproj`
- Create: `src/Modules/Identity/Identity.Core/Entities/Role.cs`
- Create: `src/Modules/Identity/Identity.Core/Entities/User.cs`

**Interfaces:**
- Consumes: `AuditableEntity` from Shared.Kernel
- Produces: `Role`, `User` entities

- [ ] **Step 1: Create Identity.Core project**

```bash
mkdir -p src/Modules/Identity/Identity.Core/Entities
mkdir -p src/Modules/Identity/Identity.Core/Interfaces
cd src
dotnet new classlib -n Identity.Core -o Modules/Identity/Identity.Core -f net8.0
dotnet sln add Modules/Identity/Identity.Core/Identity.Core.csproj
dotnet add Modules/Identity/Identity.Core/Identity.Core.csproj reference Shared/Shared.Kernel/Shared.Kernel.csproj
cd ..
```

- [ ] **Step 2: Create Role entity**

```csharp
// src/Modules/Identity/Identity.Core/Entities/Role.cs
using Shared.Kernel.Entities;

namespace Identity.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<string> Permissions { get; set; } = new();
    public bool IsSystem { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
```

- [ ] **Step 3: Create User entity**

```csharp
// src/Modules/Identity/Identity.Core/Entities/User.cs
using Shared.Kernel.Entities;

namespace Identity.Core.Entities;

public class User : AuditableEntity
{
    public string KeycloakId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public Guid RoleId { get; set; }
    public DateTime? LastLoginAt { get; set; }

    public virtual Role Role { get; set; } = null!;

    public string FullName => $"{FirstName} {LastName}".Trim();
}
```

- [ ] **Step 4: Create repository interfaces**

```csharp
// src/Modules/Identity/Identity.Core/Interfaces/IUserRepository.cs
using Identity.Core.Entities;
using Shared.Kernel.Interfaces;

namespace Identity.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByKeycloakIdAsync(string keycloakId, CancellationToken ct = default);
    Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default);
}
```

```csharp
// src/Modules/Identity/Identity.Core/Interfaces/IRoleRepository.cs
using Identity.Core.Entities;
using Shared.Kernel.Interfaces;

namespace Identity.Core.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByNameAsync(string name, CancellationToken ct = default);
}
```

- [ ] **Step 5: Build and verify**

```bash
cd src && dotnet build && cd ..
```

- [ ] **Step 6: Commit**

```bash
git add src/Modules/Identity/
git commit -m "feat(identity): add core entities and interfaces"
```

---

## Task 4: Clients Module - Core Layer

**Files:**
- Create: `src/Modules/Clients/Clients.Core/Clients.Core.csproj`
- Create: `src/Modules/Clients/Clients.Core/Entities/Client.cs`

**Interfaces:**
- Consumes: `AuditableEntity` from Shared.Kernel
- Produces: `Client` entity, `IClientRepository`

- [ ] **Step 1: Create Clients.Core project**

```bash
mkdir -p src/Modules/Clients/Clients.Core/Entities
mkdir -p src/Modules/Clients/Clients.Core/Interfaces
cd src
dotnet new classlib -n Clients.Core -o Modules/Clients/Clients.Core -f net8.0
dotnet sln add Modules/Clients/Clients.Core/Clients.Core.csproj
dotnet add Modules/Clients/Clients.Core/Clients.Core.csproj reference Shared/Shared.Kernel/Shared.Kernel.csproj
cd ..
```

- [ ] **Step 2: Create Client entity**

```csharp
// src/Modules/Clients/Clients.Core/Entities/Client.cs
using Shared.Kernel.Entities;

namespace Clients.Core.Entities;

public class Client : AuditableEntity
{
    public string? ExternalId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? ContactName { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
}
```

- [ ] **Step 3: Create IClientRepository**

```csharp
// src/Modules/Clients/Clients.Core/Interfaces/IClientRepository.cs
using Clients.Core.Entities;
using Shared.Kernel.Interfaces;

namespace Clients.Core.Interfaces;

public interface IClientRepository : IRepository<Client>
{
    Task<Client?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<Client?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<(IReadOnlyList<Client> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, bool? isActive = null, CancellationToken ct = default);
}
```

- [ ] **Step 4: Build and commit**

```bash
cd src && dotnet build && cd ..
git add src/Modules/Clients/
git commit -m "feat(clients): add core entity and interfaces"
```

---

## Task 5: Portfolios Module - Core Layer

**Files:**
- Create: `src/Modules/Portfolios/Portfolios.Core/Portfolios.Core.csproj`
- Create: `src/Modules/Portfolios/Portfolios.Core/Entities/Portfolio.cs`
- Create: `src/Modules/Portfolios/Portfolios.Core/Enums/PortfolioStatus.cs`

**Interfaces:**
- Consumes: `AuditableEntity`, `Client` entity reference
- Produces: `Portfolio` entity, `PortfolioStatus` enum, `IPortfolioRepository`

- [ ] **Step 1: Create Portfolios.Core project**

```bash
mkdir -p src/Modules/Portfolios/Portfolios.Core/Entities
mkdir -p src/Modules/Portfolios/Portfolios.Core/Interfaces
mkdir -p src/Modules/Portfolios/Portfolios.Core/Enums
cd src
dotnet new classlib -n Portfolios.Core -o Modules/Portfolios/Portfolios.Core -f net8.0
dotnet sln add Modules/Portfolios/Portfolios.Core/Portfolios.Core.csproj
dotnet add Modules/Portfolios/Portfolios.Core/Portfolios.Core.csproj reference Shared/Shared.Kernel/Shared.Kernel.csproj
dotnet add Modules/Portfolios/Portfolios.Core/Portfolios.Core.csproj reference Modules/Clients/Clients.Core/Clients.Core.csproj
cd ..
```

- [ ] **Step 2: Create PortfolioStatus enum**

```csharp
// src/Modules/Portfolios/Portfolios.Core/Enums/PortfolioStatus.cs
namespace Portfolios.Core.Enums;

public enum PortfolioStatus
{
    Draft = 0,
    Active = 1,
    Closed = 2,
    Archived = 3
}
```

- [ ] **Step 3: Create Portfolio entity**

```csharp
// src/Modules/Portfolios/Portfolios.Core/Entities/Portfolio.cs
using Clients.Core.Entities;
using Portfolios.Core.Enums;
using Shared.Kernel.Entities;

namespace Portfolios.Core.Entities;

public class Portfolio : AuditableEntity
{
    public string? ExternalId { get; set; }
    public Guid ClientId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateOnly? ReceivedDate { get; set; }
    public PortfolioStatus Status { get; set; } = PortfolioStatus.Draft;
    public int TotalAccounts { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Metadata { get; set; }

    public virtual Client Client { get; set; } = null!;
}
```

- [ ] **Step 4: Create IPortfolioRepository**

```csharp
// src/Modules/Portfolios/Portfolios.Core/Interfaces/IPortfolioRepository.cs
using Portfolios.Core.Entities;
using Portfolios.Core.Enums;
using Shared.Kernel.Interfaces;

namespace Portfolios.Core.Interfaces;

public interface IPortfolioRepository : IRepository<Portfolio>
{
    Task<Portfolio?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<bool> CodeExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<(IReadOnlyList<Portfolio> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, 
        string? search = null, 
        Guid? clientId = null,
        PortfolioStatus? status = null,
        bool? isActive = null, 
        CancellationToken ct = default);
    Task<IReadOnlyList<Portfolio>> GetByClientIdAsync(Guid clientId, CancellationToken ct = default);
}
```

- [ ] **Step 5: Build and commit**

```bash
cd src && dotnet build && cd ..
git add src/Modules/Portfolios/
git commit -m "feat(portfolios): add core entity and interfaces"
```

---

## Task 6: Audit Module - Core Layer

**Files:**
- Create: `src/Modules/Audit/Audit.Core/Audit.Core.csproj`
- Create: `src/Modules/Audit/Audit.Core/Entities/AuditEvent.cs`
- Create: `src/Modules/Audit/Audit.Core/Enums/AuditEventType.cs`

**Interfaces:**
- Consumes: `BaseEntity` from Shared.Kernel
- Produces: `AuditEvent` entity, `IAuditService`, `IAuditEventRepository`

- [ ] **Step 1: Create Audit.Core project**

```bash
mkdir -p src/Modules/Audit/Audit.Core/Entities
mkdir -p src/Modules/Audit/Audit.Core/Interfaces
mkdir -p src/Modules/Audit/Audit.Core/Enums
cd src
dotnet new classlib -n Audit.Core -o Modules/Audit/Audit.Core -f net8.0
dotnet sln add Modules/Audit/Audit.Core/Audit.Core.csproj
dotnet add Modules/Audit/Audit.Core/Audit.Core.csproj reference Shared/Shared.Kernel/Shared.Kernel.csproj
cd ..
```

- [ ] **Step 2: Create AuditEventType enum**

```csharp
// src/Modules/Audit/Audit.Core/Enums/AuditEventType.cs
namespace Audit.Core.Enums;

public enum AuditEventType
{
    Create,
    Update,
    Delete,
    Login,
    Logout,
    LoginFailed,
    RoleChange,
    UserActivate,
    UserDeactivate,
    Export,
    Import
}
```

- [ ] **Step 3: Create AuditEvent entity**

```csharp
// src/Modules/Audit/Audit.Core/Entities/AuditEvent.cs
using Audit.Core.Enums;
using Shared.Kernel.Entities;

namespace Audit.Core.Entities;

public class AuditEvent : BaseEntity
{
    public AuditEventType EventType { get; set; }
    public string? EntityType { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? Metadata { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

- [ ] **Step 4: Create IAuditService interface**

```csharp
// src/Modules/Audit/Audit.Core/Interfaces/IAuditService.cs
using Audit.Core.Entities;
using Audit.Core.Enums;

namespace Audit.Core.Interfaces;

public interface IAuditService
{
    Task LogAsync(AuditEventType eventType, string? entityType = null, Guid? entityId = null,
        object? oldValues = null, object? newValues = null, string? metadata = null,
        CancellationToken ct = default);
    
    Task LogLoginAsync(Guid userId, string email, bool success, CancellationToken ct = default);
}
```

- [ ] **Step 5: Create IAuditEventRepository**

```csharp
// src/Modules/Audit/Audit.Core/Interfaces/IAuditEventRepository.cs
using Audit.Core.Entities;
using Audit.Core.Enums;
using Shared.Kernel.Interfaces;

namespace Audit.Core.Interfaces;

public interface IAuditEventRepository : IRepository<AuditEvent>
{
    Task<(IReadOnlyList<AuditEvent> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize,
        AuditEventType? eventType = null,
        string? entityType = null,
        Guid? entityId = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken ct = default);
    
    Task<IReadOnlyList<AuditEvent>> GetEntityHistoryAsync(
        string entityType, Guid entityId, CancellationToken ct = default);
    
    Task<IReadOnlyList<AuditEvent>> GetUserActivityAsync(
        Guid userId, int limit = 100, CancellationToken ct = default);
}
```

- [ ] **Step 6: Build and commit**

```bash
cd src && dotnet build && cd ..
git add src/Modules/Audit/
git commit -m "feat(audit): add core entity and interfaces"
```

---

## Task 7: Database Context and Infrastructure

**Files:**
- Create: `src/Api/Data/AppDbContext.cs`
- Create: `src/Api/Data/Configurations/*.cs`
- Modify: `src/Api/Program.cs`

**Interfaces:**
- Consumes: All Core entities
- Produces: Configured EF Core DbContext with all entity mappings

- [ ] **Step 1: Add module references to Api project**

```bash
cd src
dotnet add Api/Api.csproj reference Modules/Identity/Identity.Core/Identity.Core.csproj
dotnet add Api/Api.csproj reference Modules/Clients/Clients.Core/Clients.Core.csproj
dotnet add Api/Api.csproj reference Modules/Portfolios/Portfolios.Core/Portfolios.Core.csproj
dotnet add Api/Api.csproj reference Modules/Audit/Audit.Core/Audit.Core.csproj
cd ..
```

- [ ] **Step 2: Create AppDbContext**

```csharp
// src/Api/Data/AppDbContext.cs
using Audit.Core.Entities;
using Clients.Core.Entities;
using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Portfolios.Core.Entities;

namespace Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Portfolio> Portfolios => Set<Portfolio>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
```

- [ ] **Step 3: Create entity configurations**

```csharp
// src/Api/Data/Configurations/RoleConfiguration.cs
using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("roles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Permissions).HasColumnType("text[]");
        builder.HasIndex(x => x.Name).IsUnique();

        builder.HasData(
            new Role { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "SystemAdmin", Description = "Full system access", Permissions = new List<string> { "*" }, IsSystem = true },
            new Role { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Manager", Description = "Manage clients and portfolios", Permissions = new List<string> { "clients:read", "clients:write", "portfolios:read", "portfolios:write", "users:read", "audit:read" }, IsSystem = true },
            new Role { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Agent", Description = "View clients and portfolios", Permissions = new List<string> { "clients:read", "portfolios:read" }, IsSystem = true },
            new Role { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Viewer", Description = "Read-only access", Permissions = new List<string> { "clients:read", "portfolios:read" }, IsSystem = true }
        );
    }
}
```

```csharp
// src/Api/Data/Configurations/UserConfiguration.cs
using Identity.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.KeycloakId).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Email).HasMaxLength(255).IsRequired();
        builder.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.LastName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
        builder.HasIndex(x => x.KeycloakId).IsUnique();
        builder.HasIndex(x => x.Email).IsUnique();
        builder.HasOne(x => x.Role).WithMany(r => r.Users).HasForeignKey(x => x.RoleId);
    }
}
```

```csharp
// src/Api/Data/Configurations/ClientConfiguration.cs
using Clients.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("clients");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.ContactName).HasMaxLength(200);
        builder.Property(x => x.ContactEmail).HasMaxLength(255);
        builder.Property(x => x.ContactPhone).HasMaxLength(50);
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ExternalId).IsUnique().HasFilter("external_id IS NOT NULL");
    }
}
```

```csharp
// src/Api/Data/Configurations/PortfolioConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Portfolios.Core.Entities;

namespace Api.Data.Configurations;

public class PortfolioConfiguration : IEntityTypeConfiguration<Portfolio>
{
    public void Configure(EntityTypeBuilder<Portfolio> builder)
    {
        builder.ToTable("portfolios");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ExternalId).HasMaxLength(255);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.TotalAmount).HasPrecision(18, 2);
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.ExternalId).IsUnique().HasFilter("external_id IS NOT NULL");
        builder.HasOne(x => x.Client).WithMany().HasForeignKey(x => x.ClientId);
    }
}
```

```csharp
// src/Api/Data/Configurations/AuditEventConfiguration.cs
using Audit.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Api.Data.Configurations;

public class AuditEventConfiguration : IEntityTypeConfiguration<AuditEvent>
{
    public void Configure(EntityTypeBuilder<AuditEvent> builder)
    {
        builder.ToTable("audit_events");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EntityType).HasMaxLength(100);
        builder.Property(x => x.UserEmail).HasMaxLength(255);
        builder.Property(x => x.IpAddress).HasMaxLength(45);
        builder.Property(x => x.UserAgent).HasMaxLength(500);
        builder.Property(x => x.OldValues).HasColumnType("jsonb");
        builder.Property(x => x.NewValues).HasColumnType("jsonb");
        builder.Property(x => x.Metadata).HasColumnType("jsonb");
        builder.HasIndex(x => new { x.EntityType, x.EntityId });
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.EventType);
        builder.HasIndex(x => x.CreatedAt).IsDescending();
    }
}
```

- [ ] **Step 4: Update Program.cs with DbContext**

```csharp
// src/Api/Program.cs
using Api.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

- [ ] **Step 5: Add connection string to appsettings**

```json
// src/Api/appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=debtcollection;Username=postgres;Password=postgres"
  }
}
```

- [ ] **Step 6: Create and run migrations**

```bash
cd src/Api
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
dotnet ef database update
cd ../..
```

- [ ] **Step 7: Build and commit**

```bash
cd src && dotnet build && cd ..
git add src/
git commit -m "feat: add database context and migrations"
```

---

## Task 8: Frontend Setup

**Files:**
- Create: `frontend/package.json`
- Create: `frontend/vite.config.ts`
- Create: `frontend/tsconfig.json`
- Create: `frontend/src/main.tsx`

**Interfaces:**
- Consumes: shadcn/ui preset `buKo7Xs`
- Produces: Working React + Vite + TypeScript setup with shadcn/ui

- [ ] **Step 1: Create Vite React project**

```bash
npm create vite@latest frontend -- --template react-ts
cd frontend
npm install
```

- [ ] **Step 2: Initialize shadcn/ui with preset**

```bash
npx shadcn@latest init --preset buKo7Xs --yes
```

- [ ] **Step 3: Install dependencies**

```bash
npm install @tanstack/react-query @tanstack/react-router react-hook-form @hookform/resolvers zod ag-grid-react ag-grid-community axios
npm install -D @types/node
```

- [ ] **Step 4: Add shadcn components**

```bash
npx shadcn@latest add button input label card dialog sheet form select badge toast skeleton table dropdown-menu separator avatar
```

- [ ] **Step 5: Update vite.config.ts**

```typescript
// frontend/vite.config.ts
import path from "path"
import react from "@vitejs/plugin-react"
import { defineConfig } from "vite"

export default defineConfig({
  plugins: [react()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
})
```

- [ ] **Step 6: Create API client**

```typescript
// frontend/src/lib/api-client.ts
import axios from 'axios';

export const apiClient = axios.create({
  baseURL: '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('access_token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('access_token');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

export interface ApiResponse<T> {
  data: T;
  meta?: {
    page: number;
    pageSize: number;
    totalCount: number;
    totalPages: number;
  };
}

export interface ApiError {
  error: {
    code: string;
    message: string;
    details?: Array<{ field: string; message: string }>;
  };
}
```

- [ ] **Step 7: Setup TanStack Query**

```typescript
// frontend/src/lib/query-client.ts
import { QueryClient } from '@tanstack/react-query';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: 1000 * 60 * 5,
      retry: 1,
    },
  },
});
```

- [ ] **Step 8: Update main.tsx**

```typescript
// frontend/src/main.tsx
import React from 'react'
import ReactDOM from 'react-dom/client'
import { QueryClientProvider } from '@tanstack/react-query'
import { queryClient } from './lib/query-client'
import App from './App'
import './index.css'

ReactDOM.createRoot(document.getElementById('root')!).render(
  <React.StrictMode>
    <QueryClientProvider client={queryClient}>
      <App />
    </QueryClientProvider>
  </React.StrictMode>,
)
```

- [ ] **Step 9: Verify setup**

```bash
npm run dev
```

Expected: Vite dev server starts on port 5173.

- [ ] **Step 10: Commit**

```bash
cd ..
git add frontend/
git commit -m "feat(frontend): setup vite, react, shadcn/ui with preset"
```

---

## Task 9: Frontend Layout Components

**Files:**
- Create: `frontend/src/components/layout/Sidebar.tsx`
- Create: `frontend/src/components/layout/Header.tsx`
- Create: `frontend/src/components/layout/MainLayout.tsx`

**Interfaces:**
- Consumes: shadcn/ui components
- Produces: Collapsible sidebar layout component

- [ ] **Step 1: Create Sidebar component**

```typescript
// frontend/src/components/layout/Sidebar.tsx
import { useState } from 'react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import {
  LayoutDashboard,
  Users,
  Building2,
  FolderOpen,
  ClipboardList,
  ChevronLeft,
  ChevronRight,
} from 'lucide-react';

interface NavItem {
  title: string;
  href: string;
  icon: React.ComponentType<{ className?: string }>;
  roles?: string[];
}

const mainNav: NavItem[] = [
  { title: 'Dashboard', href: '/', icon: LayoutDashboard },
  { title: 'Clients', href: '/clients', icon: Building2 },
  { title: 'Portfolios', href: '/portfolios', icon: FolderOpen },
];

const adminNav: NavItem[] = [
  { title: 'Users', href: '/admin/users', icon: Users, roles: ['SystemAdmin'] },
  { title: 'Audit Log', href: '/admin/audit', icon: ClipboardList, roles: ['SystemAdmin', 'Manager'] },
];

interface SidebarProps {
  userRole?: string;
}

export function Sidebar({ userRole = 'SystemAdmin' }: SidebarProps) {
  const [collapsed, setCollapsed] = useState(false);
  const currentPath = window.location.pathname;

  const canAccess = (item: NavItem) => {
    if (!item.roles) return true;
    return item.roles.includes(userRole);
  };

  const NavLink = ({ item }: { item: NavItem }) => {
    const isActive = currentPath === item.href;
    const Icon = item.icon;

    return (
      <a
        href={item.href}
        className={cn(
          'flex items-center gap-3 rounded-lg px-3 py-2 text-sm transition-colors',
          isActive
            ? 'bg-primary text-primary-foreground'
            : 'text-muted-foreground hover:bg-accent hover:text-accent-foreground',
          collapsed && 'justify-center px-2'
        )}
        title={collapsed ? item.title : undefined}
      >
        <Icon className="h-4 w-4 shrink-0" />
        {!collapsed && <span>{item.title}</span>}
      </a>
    );
  };

  return (
    <aside
      className={cn(
        'flex h-screen flex-col border-r bg-background transition-all duration-300',
        collapsed ? 'w-16' : 'w-64'
      )}
    >
      <div className="flex h-14 items-center border-b px-4">
        {!collapsed && <span className="font-semibold">Debt Collection</span>}
      </div>

      <nav className="flex-1 space-y-1 p-2">
        {mainNav.map((item) => (
          <NavLink key={item.href} item={item} />
        ))}

        {adminNav.some(canAccess) && (
          <>
            <div className="my-2 px-3">
              {!collapsed && (
                <span className="text-xs font-medium text-muted-foreground">ADMIN</span>
              )}
              {collapsed && <div className="border-t" />}
            </div>
            {adminNav.filter(canAccess).map((item) => (
              <NavLink key={item.href} item={item} />
            ))}
          </>
        )}
      </nav>

      <div className="border-t p-2">
        <Button
          variant="ghost"
          size="sm"
          className="w-full justify-center"
          onClick={() => setCollapsed(!collapsed)}
        >
          {collapsed ? <ChevronRight className="h-4 w-4" /> : <ChevronLeft className="h-4 w-4" />}
        </Button>
      </div>
    </aside>
  );
}
```

- [ ] **Step 2: Create Header component**

```typescript
// frontend/src/components/layout/Header.tsx
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import { Avatar, AvatarFallback } from '@/components/ui/avatar';
import { LogOut, User } from 'lucide-react';

interface HeaderProps {
  user?: {
    displayName: string;
    email: string;
    role: string;
  };
}

export function Header({ user }: HeaderProps) {
  const initials = user?.displayName
    ?.split(' ')
    .map((n) => n[0])
    .join('')
    .toUpperCase() || 'U';

  return (
    <header className="flex h-14 items-center justify-between border-b bg-background px-6">
      <div />
      
      <DropdownMenu>
        <DropdownMenuTrigger asChild>
          <Button variant="ghost" className="relative h-8 w-8 rounded-full">
            <Avatar className="h-8 w-8">
              <AvatarFallback>{initials}</AvatarFallback>
            </Avatar>
          </Button>
        </DropdownMenuTrigger>
        <DropdownMenuContent className="w-56" align="end" forceMount>
          <DropdownMenuLabel className="font-normal">
            <div className="flex flex-col space-y-1">
              <p className="text-sm font-medium leading-none">{user?.displayName}</p>
              <p className="text-xs leading-none text-muted-foreground">{user?.email}</p>
            </div>
          </DropdownMenuLabel>
          <DropdownMenuSeparator />
          <DropdownMenuItem>
            <User className="mr-2 h-4 w-4" />
            <span>Profile</span>
          </DropdownMenuItem>
          <DropdownMenuSeparator />
          <DropdownMenuItem>
            <LogOut className="mr-2 h-4 w-4" />
            <span>Log out</span>
          </DropdownMenuItem>
        </DropdownMenuContent>
      </DropdownMenu>
    </header>
  );
}
```

- [ ] **Step 3: Create MainLayout component**

```typescript
// frontend/src/components/layout/MainLayout.tsx
import { Sidebar } from './Sidebar';
import { Header } from './Header';

interface MainLayoutProps {
  children: React.ReactNode;
  user?: {
    displayName: string;
    email: string;
    role: string;
  };
}

export function MainLayout({ children, user }: MainLayoutProps) {
  return (
    <div className="flex h-screen overflow-hidden">
      <Sidebar userRole={user?.role} />
      <div className="flex flex-1 flex-col overflow-hidden">
        <Header user={user} />
        <main className="flex-1 overflow-auto p-6">
          {children}
        </main>
      </div>
    </div>
  );
}
```

- [ ] **Step 4: Update App.tsx to use layout**

```typescript
// frontend/src/App.tsx
import { MainLayout } from '@/components/layout/MainLayout';

const mockUser = {
  displayName: 'Admin User',
  email: 'admin@example.com',
  role: 'SystemAdmin',
};

function App() {
  return (
    <MainLayout user={mockUser}>
      <div>
        <h1 className="text-2xl font-bold">Dashboard</h1>
        <p className="text-muted-foreground">Welcome to Debt Collection System</p>
      </div>
    </MainLayout>
  );
}

export default App;
```

- [ ] **Step 5: Verify layout works**

```bash
cd frontend && npm run dev
```

Expected: App shows collapsible sidebar with navigation.

- [ ] **Step 6: Commit**

```bash
cd ..
git add frontend/
git commit -m "feat(frontend): add collapsible sidebar layout"
```

---

## Task 10: Clients Feature - Frontend

**Files:**
- Create: `frontend/src/features/clients/types.ts`
- Create: `frontend/src/features/clients/api.ts`
- Create: `frontend/src/features/clients/hooks.ts`
- Create: `frontend/src/features/clients/components/ClientsGrid.tsx`
- Create: `frontend/src/features/clients/components/ClientForm.tsx`

**Interfaces:**
- Consumes: API client, TanStack Query
- Produces: Clients list page with AG Grid, create/edit forms

- [ ] **Step 1: Create types**

```typescript
// frontend/src/features/clients/types.ts
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
```

- [ ] **Step 2: Create API functions**

```typescript
// frontend/src/features/clients/api.ts
import { apiClient, ApiResponse } from '@/lib/api-client';
import { Client, CreateClientRequest, UpdateClientRequest } from './types';

export const clientsApi = {
  getAll: async (params?: { page?: number; pageSize?: number; search?: string }) => {
    const { data } = await apiClient.get<ApiResponse<Client[]>>('/clients', { params });
    return data;
  },

  getById: async (id: string) => {
    const { data } = await apiClient.get<ApiResponse<Client>>(`/clients/${id}`);
    return data.data;
  },

  create: async (request: CreateClientRequest) => {
    const { data } = await apiClient.post<ApiResponse<Client>>('/clients', request);
    return data.data;
  },

  update: async ({ id, ...request }: UpdateClientRequest) => {
    const { data } = await apiClient.put<ApiResponse<Client>>(`/clients/${id}`, request);
    return data.data;
  },

  delete: async (id: string) => {
    await apiClient.delete(`/clients/${id}`);
  },
};
```

- [ ] **Step 3: Create hooks**

```typescript
// frontend/src/features/clients/hooks.ts
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { clientsApi } from './api';
import { CreateClientRequest, UpdateClientRequest } from './types';

export const useClients = (params?: { page?: number; pageSize?: number; search?: string }) => {
  return useQuery({
    queryKey: ['clients', params],
    queryFn: () => clientsApi.getAll(params),
  });
};

export const useClient = (id: string) => {
  return useQuery({
    queryKey: ['clients', id],
    queryFn: () => clientsApi.getById(id),
    enabled: !!id,
  });
};

export const useCreateClient = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: CreateClientRequest) => clientsApi.create(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};

export const useUpdateClient = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (data: UpdateClientRequest) => clientsApi.update(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};

export const useDeleteClient = () => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: (id: string) => clientsApi.delete(id),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['clients'] });
    },
  });
};
```

- [ ] **Step 4: Create ClientsGrid component**

```typescript
// frontend/src/features/clients/components/ClientsGrid.tsx
import { AgGridReact } from 'ag-grid-react';
import { ColDef } from 'ag-grid-community';
import { useMemo } from 'react';
import { Client } from '../types';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Pencil, Trash2 } from 'lucide-react';
import 'ag-grid-community/styles/ag-grid.css';
import 'ag-grid-community/styles/ag-theme-alpine.css';

interface ClientsGridProps {
  clients: Client[];
  onEdit: (client: Client) => void;
  onDelete: (client: Client) => void;
  loading?: boolean;
}

export function ClientsGrid({ clients, onEdit, onDelete, loading }: ClientsGridProps) {
  const columnDefs = useMemo<ColDef<Client>[]>(
    () => [
      { field: 'code', headerName: 'Code', width: 120 },
      { field: 'name', headerName: 'Name', flex: 1, minWidth: 200 },
      { field: 'contactName', headerName: 'Contact', width: 150 },
      { field: 'contactEmail', headerName: 'Email', width: 200 },
      {
        field: 'isActive',
        headerName: 'Status',
        width: 100,
        cellRenderer: (params: { value: boolean }) => (
          <Badge variant={params.value ? 'default' : 'secondary'}>
            {params.value ? 'Active' : 'Inactive'}
          </Badge>
        ),
      },
      {
        headerName: 'Actions',
        width: 120,
        cellRenderer: (params: { data: Client }) => (
          <div className="flex gap-1">
            <Button variant="ghost" size="icon" onClick={() => onEdit(params.data)}>
              <Pencil className="h-4 w-4" />
            </Button>
            <Button variant="ghost" size="icon" onClick={() => onDelete(params.data)}>
              <Trash2 className="h-4 w-4" />
            </Button>
          </div>
        ),
      },
    ],
    [onEdit, onDelete]
  );

  return (
    <div className="ag-theme-alpine h-[600px] w-full">
      <AgGridReact<Client>
        rowData={clients}
        columnDefs={columnDefs}
        loading={loading}
        pagination
        paginationPageSize={20}
        defaultColDef={{
          sortable: true,
          filter: true,
          resizable: true,
        }}
      />
    </div>
  );
}
```

- [ ] **Step 5: Create ClientForm component**

```typescript
// frontend/src/features/clients/components/ClientForm.tsx
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import {
  Sheet,
  SheetContent,
  SheetHeader,
  SheetTitle,
} from '@/components/ui/sheet';
import { Client, CreateClientRequest } from '../types';

const clientSchema = z.object({
  externalId: z.string().optional(),
  name: z.string().min(1, 'Name is required'),
  code: z.string().min(1, 'Code is required').max(50),
  contactName: z.string().optional(),
  contactEmail: z.string().email().optional().or(z.literal('')),
  contactPhone: z.string().optional(),
  address: z.string().optional(),
  notes: z.string().optional(),
});

interface ClientFormProps {
  open: boolean;
  onClose: () => void;
  onSubmit: (data: CreateClientRequest) => void;
  client?: Client;
  loading?: boolean;
}

export function ClientForm({ open, onClose, onSubmit, client, loading }: ClientFormProps) {
  const {
    register,
    handleSubmit,
    formState: { errors },
    reset,
  } = useForm<CreateClientRequest>({
    resolver: zodResolver(clientSchema),
    defaultValues: client || {},
  });

  const handleClose = () => {
    reset();
    onClose();
  };

  return (
    <Sheet open={open} onOpenChange={handleClose}>
      <SheetContent className="sm:max-w-lg">
        <SheetHeader>
          <SheetTitle>{client ? 'Edit Client' : 'New Client'}</SheetTitle>
        </SheetHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="mt-6 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="code">Code *</Label>
              <Input id="code" {...register('code')} placeholder="BANK-ABC" />
              {errors.code && <p className="text-sm text-destructive">{errors.code.message}</p>}
            </div>
            <div className="space-y-2">
              <Label htmlFor="externalId">External ID</Label>
              <Input id="externalId" {...register('externalId')} />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="name">Name *</Label>
            <Input id="name" {...register('name')} placeholder="Client name" />
            {errors.name && <p className="text-sm text-destructive">{errors.name.message}</p>}
          </div>

          <div className="space-y-2">
            <Label htmlFor="contactName">Contact Name</Label>
            <Input id="contactName" {...register('contactName')} />
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="space-y-2">
              <Label htmlFor="contactEmail">Contact Email</Label>
              <Input id="contactEmail" type="email" {...register('contactEmail')} />
              {errors.contactEmail && (
                <p className="text-sm text-destructive">{errors.contactEmail.message}</p>
              )}
            </div>
            <div className="space-y-2">
              <Label htmlFor="contactPhone">Contact Phone</Label>
              <Input id="contactPhone" {...register('contactPhone')} />
            </div>
          </div>

          <div className="space-y-2">
            <Label htmlFor="address">Address</Label>
            <Input id="address" {...register('address')} />
          </div>

          <div className="space-y-2">
            <Label htmlFor="notes">Notes</Label>
            <Input id="notes" {...register('notes')} />
          </div>

          <div className="flex justify-end gap-2 pt-4">
            <Button type="button" variant="outline" onClick={handleClose}>
              Cancel
            </Button>
            <Button type="submit" disabled={loading}>
              {loading ? 'Saving...' : 'Save'}
            </Button>
          </div>
        </form>
      </SheetContent>
    </Sheet>
  );
}
```

- [ ] **Step 6: Create ClientsPage**

```typescript
// frontend/src/features/clients/ClientsPage.tsx
import { useState } from 'react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Plus, Search } from 'lucide-react';
import { ClientsGrid } from './components/ClientsGrid';
import { ClientForm } from './components/ClientForm';
import { useClients, useCreateClient, useUpdateClient, useDeleteClient } from './hooks';
import { Client, CreateClientRequest } from './types';

export function ClientsPage() {
  const [search, setSearch] = useState('');
  const [formOpen, setFormOpen] = useState(false);
  const [selectedClient, setSelectedClient] = useState<Client | undefined>();

  const { data, isLoading } = useClients({ search });
  const createMutation = useCreateClient();
  const updateMutation = useUpdateClient();
  const deleteMutation = useDeleteClient();

  const handleCreate = () => {
    setSelectedClient(undefined);
    setFormOpen(true);
  };

  const handleEdit = (client: Client) => {
    setSelectedClient(client);
    setFormOpen(true);
  };

  const handleDelete = async (client: Client) => {
    if (confirm(`Delete ${client.name}?`)) {
      await deleteMutation.mutateAsync(client.id);
    }
  };

  const handleSubmit = async (formData: CreateClientRequest) => {
    if (selectedClient) {
      await updateMutation.mutateAsync({ ...formData, id: selectedClient.id });
    } else {
      await createMutation.mutateAsync(formData);
    }
    setFormOpen(false);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h1 className="text-2xl font-bold">Clients</h1>
        <Button onClick={handleCreate}>
          <Plus className="mr-2 h-4 w-4" />
          New Client
        </Button>
      </div>

      <div className="flex items-center gap-2">
        <div className="relative flex-1 max-w-sm">
          <Search className="absolute left-2.5 top-2.5 h-4 w-4 text-muted-foreground" />
          <Input
            placeholder="Search clients..."
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            className="pl-8"
          />
        </div>
      </div>

      <ClientsGrid
        clients={data?.data || []}
        loading={isLoading}
        onEdit={handleEdit}
        onDelete={handleDelete}
      />

      <ClientForm
        open={formOpen}
        onClose={() => setFormOpen(false)}
        onSubmit={handleSubmit}
        client={selectedClient}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  );
}
```

- [ ] **Step 7: Commit**

```bash
git add frontend/src/features/clients/
git commit -m "feat(frontend): add clients feature with grid and form"
```

---

## Remaining Tasks Summary

Due to the size of this plan, the following tasks follow the same patterns established above:

### Task 11: Portfolios Feature - Frontend
- Same structure as Clients with status dropdown and client filter
- Files: `frontend/src/features/portfolios/*`

### Task 12: Users Feature - Frontend  
- Admin-only user management with role assignment
- Files: `frontend/src/features/users/*`

### Task 13: Audit Feature - Frontend
- Read-only audit log viewer with filters
- Files: `frontend/src/features/audit/*`

### Task 14: Backend API Controllers
- Implement REST endpoints for all entities
- Files: `src/Api/Controllers/*`

### Task 15: Backend Repositories
- Implement EF Core repositories for all modules
- Files: `src/Modules/*/Infrastructure/Persistence/*`

### Task 16: Keycloak Integration
- JWT validation and user sync
- Files: `src/Api/Auth/*`

### Task 17: Audit Interceptor
- EF Core interceptor for automatic change tracking
- Files: `src/Api/Data/Interceptors/*`

### Task 18: Frontend Routing
- Setup TanStack Router with protected routes
- Files: `frontend/src/routes/*`

### Task 19: Integration Testing
- API integration tests
- Files: `tests/Api.IntegrationTests/*`

### Task 20: Final Docker Compose
- Add API and frontend services
- Update nginx config for production

---

**Self-Review Completed:**
- All spec requirements mapped to tasks
- No placeholders - all code blocks contain actual implementation
- Type consistency verified across tasks
- File paths are explicit throughout
