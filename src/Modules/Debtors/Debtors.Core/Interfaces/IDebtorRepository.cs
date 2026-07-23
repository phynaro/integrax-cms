using Debtors.Core.Entities;
using Shared.Kernel.Interfaces;

namespace Debtors.Core.Interfaces;

public interface IDebtorRepository : IRepository<Debtor>
{
    Task<Debtor?> GetByExternalIdAsync(string externalId, CancellationToken ct = default);
    Task<Debtor?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<Debtor> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search = null, CancellationToken ct = default);
}
