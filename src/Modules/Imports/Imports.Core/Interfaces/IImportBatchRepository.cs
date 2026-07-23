using Imports.Core.Entities;
using Imports.Core.Enums;
using Shared.Kernel.Interfaces;

namespace Imports.Core.Interfaces;

public interface IImportBatchRepository : IRepository<ImportBatch>
{
    Task<ImportBatch?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<(IReadOnlyList<ImportBatch> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? portfolioId = null, ImportStatus? status = null, 
        CancellationToken ct = default);
}
