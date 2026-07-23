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
