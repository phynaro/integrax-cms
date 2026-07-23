using Cases.Core.Entities;
using Cases.Core.Enums;
using Shared.Kernel.Interfaces;

namespace Cases.Core.Interfaces;

public interface ICaseRepository : IRepository<CollectionCase>
{
    Task<CollectionCase?> GetByCaseNumberAsync(string caseNumber, CancellationToken ct = default);
    Task<CollectionCase?> GetByDebtAccountIdAsync(Guid debtAccountId, CancellationToken ct = default);
    Task<CollectionCase?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<CollectionCase>> GetByAssignedToAsync(Guid userId, CancellationToken ct = default);
    Task<(IReadOnlyList<CollectionCase> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? assignedToId = null, CaseStatus? status = null,
        CasePriority? priority = null, string? search = null, CancellationToken ct = default);
    Task<string> GenerateCaseNumberAsync(CancellationToken ct = default);
}
