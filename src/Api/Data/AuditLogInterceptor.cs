using System.Text.Json;
using Api.Auth;
using Audit.Core.Entities;
using Audit.Core.Enums;
using Audit.Core.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Shared.Kernel.Entities;

namespace Api.Data;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AuditLogInterceptor> _logger;
    private List<AuditEntry>? _pendingAuditEntries;

    public AuditLogInterceptor(
        IServiceProvider serviceProvider,
        ICurrentUserService currentUserService,
        ILogger<AuditLogInterceptor> logger)
    {
        _serviceProvider = serviceProvider;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        _pendingAuditEntries = OnBeforeSaveChanges(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        OnAfterSaveChanges(_pendingAuditEntries).GetAwaiter().GetResult();
        return base.SavedChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        _pendingAuditEntries = OnBeforeSaveChanges(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        await OnAfterSaveChanges(_pendingAuditEntries);
        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private List<AuditEntry> OnBeforeSaveChanges(DbContext? context)
    {
        var auditEntries = new List<AuditEntry>();
        
        if (context == null) return auditEntries;

        context.ChangeTracker.DetectChanges();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is AuditEvent || 
                entry.State == EntityState.Detached || 
                entry.State == EntityState.Unchanged)
            {
                continue;
            }

            var auditEntry = new AuditEntry(entry)
            {
                TableName = entry.Entity.GetType().Name,
                UserId = _currentUserService.UserId,
                Action = entry.State switch
                {
                    EntityState.Added => "Create",
                    EntityState.Modified => "Update",
                    EntityState.Deleted => "Delete",
                    _ => entry.State.ToString()
                }
            };

            auditEntries.Add(auditEntry);

            foreach (var property in entry.Properties)
            {
                if (property.IsTemporary)
                {
                    auditEntry.TemporaryProperties.Add(property);
                    continue;
                }

                var propertyName = property.Metadata.Name;

                if (property.Metadata.IsPrimaryKey())
                {
                    auditEntry.KeyValues[propertyName] = property.CurrentValue;
                    continue;
                }

                switch (entry.State)
                {
                    case EntityState.Added:
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        break;

                    case EntityState.Modified:
                        if (property.IsModified && !Equals(property.OriginalValue, property.CurrentValue))
                        {
                            auditEntry.OldValues[propertyName] = property.OriginalValue;
                            auditEntry.NewValues[propertyName] = property.CurrentValue;
                        }
                        break;
                }
            }
        }

        foreach (var auditEntry in auditEntries.Where(e => !e.HasTemporaryProperties))
        {
            auditEntries.Add(auditEntry);
        }

        return auditEntries.Where(e => e.HasChanges).ToList();
    }

    private async Task OnAfterSaveChanges(List<AuditEntry>? auditEntries)
    {
        if (auditEntries == null || auditEntries.Count == 0)
            return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var auditRepository = scope.ServiceProvider.GetService<IAuditEventRepository>();
            
            if (auditRepository == null)
            {
                _logger.LogWarning("Audit repository not available for logging changes");
                return;
            }

            foreach (var auditEntry in auditEntries)
            {
                foreach (var prop in auditEntry.TemporaryProperties)
                {
                    if (prop.Metadata.IsPrimaryKey())
                    {
                        auditEntry.KeyValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                    else
                    {
                        auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                    }
                }

                var auditEvent = auditEntry.ToAuditEvent();
                await auditRepository.AddAsync(auditEvent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit logs");
        }
    }
}

public class AuditEntry
{
    public AuditEntry(EntityEntry entry)
    {
        Entry = entry;
    }

    public EntityEntry Entry { get; }
    public string TableName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Dictionary<string, object?> KeyValues { get; } = new();
    public Dictionary<string, object?> OldValues { get; } = new();
    public Dictionary<string, object?> NewValues { get; } = new();
    public List<PropertyEntry> TemporaryProperties { get; } = new();

    public bool HasTemporaryProperties => TemporaryProperties.Count > 0;
    
    public bool HasChanges => Action == "Create" || Action == "Delete" || 
                               OldValues.Count > 0 || NewValues.Count > 0;

    public AuditEvent ToAuditEvent()
    {
        var eventType = Action switch
        {
            "Create" => AuditEventType.Create,
            "Update" => AuditEventType.Update,
            "Delete" => AuditEventType.Delete,
            _ => AuditEventType.Update
        };

        Guid? entityId = null;
        if (KeyValues.TryGetValue("Id", out var id) && id is Guid guidId)
        {
            entityId = guidId;
        }

        return new AuditEvent
        {
            EntityType = TableName,
            EntityId = entityId,
            EventType = eventType,
            UserId = UserId,
            OldValues = OldValues.Count > 0 ? JsonSerializer.Serialize(OldValues) : null,
            NewValues = NewValues.Count > 0 ? JsonSerializer.Serialize(NewValues) : null,
            CreatedAt = DateTime.UtcNow
        };
    }
}
