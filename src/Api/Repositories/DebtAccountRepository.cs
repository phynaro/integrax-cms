using Api.Data;
using Accounts.Core.Entities;
using Accounts.Core.Enums;
using Accounts.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class DebtAccountRepository : BaseRepository<DebtAccount>, IDebtAccountRepository
{
    public DebtAccountRepository(AppDbContext context) : base(context) { }

    public async Task<DebtAccount?> GetByAccountNumberAsync(string accountNumber, CancellationToken ct = default)
    {
        return await _dbSet.FirstOrDefaultAsync(x => x.AccountNumber == accountNumber, ct);
    }

    public async Task<DebtAccount?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Debtor)
            .Include(x => x.Portfolio)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<IReadOnlyList<DebtAccount>> GetByDebtorIdAsync(Guid debtorId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Portfolio)
            .Where(x => x.DebtorId == debtorId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DebtAccount>> GetByPortfolioIdAsync(Guid portfolioId, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Debtor)
            .Where(x => x.PortfolioId == portfolioId)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<DebtAccount>> GetByImportBatchIdAsync(Guid importBatchId, CancellationToken ct = default)
    {
        return await _dbSet
            .Where(x => x.ImportBatchId == importBatchId)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<DebtAccount> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? portfolioId = null, Guid? debtorId = null,
        AccountStatus? status = null, string? search = null, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(x => x.Debtor)
            .Include(x => x.Portfolio)
            .AsQueryable();

        if (portfolioId.HasValue)
            query = query.Where(x => x.PortfolioId == portfolioId.Value);

        if (debtorId.HasValue)
            query = query.Where(x => x.DebtorId == debtorId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.ToLower();
            query = query.Where(x => 
                x.AccountNumber.ToLower().Contains(search) || 
                x.Debtor.DisplayName.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }

    public async Task<bool> AccountNumberExistsAsync(string accountNumber, CancellationToken ct = default)
    {
        return await _dbSet.AnyAsync(x => x.AccountNumber == accountNumber, ct);
    }
}
