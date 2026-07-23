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
