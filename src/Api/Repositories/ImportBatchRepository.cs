using Api.Data;
using Imports.Core.Entities;
using Imports.Core.Enums;
using Imports.Core.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Api.Repositories;

public class ImportBatchRepository : BaseRepository<ImportBatch>, IImportBatchRepository
{
    public ImportBatchRepository(AppDbContext context) : base(context) { }

    public async Task<ImportBatch?> GetWithDetailsAsync(Guid id, CancellationToken ct = default)
    {
        return await _dbSet
            .Include(x => x.Portfolio)
            .Include(x => x.CreatedBy)
            .Include(x => x.RolledBackBy)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
    }

    public async Task<(IReadOnlyList<ImportBatch> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, Guid? portfolioId = null, ImportStatus? status = null,
        CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(x => x.Portfolio)
            .Include(x => x.CreatedBy)
            .AsQueryable();

        if (portfolioId.HasValue)
            query = query.Where(x => x.PortfolioId == portfolioId.Value);

        if (status.HasValue)
            query = query.Where(x => x.Status == status.Value);

        var totalCount = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, totalCount);
    }
}
