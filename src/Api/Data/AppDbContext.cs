using Accounts.Core.Entities;
using Audit.Core.Entities;
using Cases.Core.Entities;
using Clients.Core.Entities;
using Debtors.Core.Entities;
using Identity.Core.Entities;
using Imports.Core.Entities;
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
    public DbSet<Debtor> Debtors => Set<Debtor>();
    public DbSet<DebtorContact> DebtorContacts => Set<DebtorContact>();
    public DbSet<DebtorAddress> DebtorAddresses => Set<DebtorAddress>();
    public DbSet<DebtAccount> DebtAccounts => Set<DebtAccount>();
    public DbSet<CollectionCase> CollectionCases => Set<CollectionCase>();
    public DbSet<ImportBatch> ImportBatches => Set<ImportBatch>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
