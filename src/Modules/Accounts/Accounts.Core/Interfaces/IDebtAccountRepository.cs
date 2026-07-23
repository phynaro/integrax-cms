using Accounts.Core.Entities;
using Accounts.Core.Enums;
using Shared.Kernel.Interfaces;

namespace Accounts.Core.Interfaces;

public interface IDebtAccountRepository : IRepository<DebtAccount>
{
    Task<DebtAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default);
    Task<DebtAccount?> GetWithDetailsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<DebtAccount>> GetByDebtorIdAsync(Guid debtorId, CancellationToken ct = default);
    Task<IReadOnlyList<DebtAccount>> GetByPortfolioIdAsync(Guid portfolioId, CancellationToken ct = default);
    Task<IReadOnlyList<DebtAccount>> GetByImportBatchIdAsync(Guid importBatchId, CancellationToken ct = default);
    Task<(IReadOnlyList<DebtAccount> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? portfolioId = null, Guid? debtorId = null, 
        AccountStatus? status = null, string? search = null, CancellationToken ct = default);
    Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken ct = default);
}
