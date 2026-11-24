using Finance.Application.Imports;
using Finance.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfImportBatchRepository(FinanceDbContext db) : IImportBatchRepository
{
    public Task<ImportBatch?> FindByChecksumAsync(string checksum, CancellationToken cancellationToken) => db.ImportBatches.Include(batch => batch.Lines).SingleOrDefaultAsync(batch => batch.Checksum == checksum, cancellationToken);
    public Task<ImportBatch?> FindByIdAsync(Guid id, CancellationToken cancellationToken) => db.ImportBatches.Include(batch => batch.Lines).SingleOrDefaultAsync(batch => batch.Id == id, cancellationToken);
    public async Task AddAsync(ImportBatch batch, CancellationToken cancellationToken) { db.ImportBatches.Add(batch); await db.SaveChangesAsync(cancellationToken); }
    public async Task<IReadOnlyList<ImportBatch>> GetAllAsync(CancellationToken cancellationToken)
    {
        var batches = await db.ImportBatches.Include(batch => batch.Lines).ToListAsync(cancellationToken);
        return batches.OrderByDescending(batch => batch.ImportedAt).ToList();
    }
}
