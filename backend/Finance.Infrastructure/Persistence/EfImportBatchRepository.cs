using Finance.Application.Imports;
using Finance.Domain.Imports;
using Microsoft.EntityFrameworkCore;
using Finance.Domain.Extensions;
using System.Text.Json;

namespace Finance.Infrastructure.Persistence;

public sealed class EfImportBatchRepository(FinanceDbContext db) : IImportBatchRepository
{
    public async Task RequireSourceModeAsync(string shop, string mode, CancellationToken ct)
    {
        var authority = await db.Set<SourceAuthority>().SingleOrDefaultAsync(a => a.ShopCode == shop, ct);
        if (mode is not ("Local" or "Connected") || mode != (authority?.Mode ?? "Local")) throw new InvalidOperationException("Writer nguồn này đã bị khóa. Dùng nguồn có thẩm quyền hiện tại.");
    }
    public async Task<bool> SourceKeyExistsAsync(string key, CancellationToken cancellationToken)
    {
        if (await db.ImportLines.AnyAsync(line => line.SourceKey == key, cancellationToken)) return true;
        // Older batches predate SourceKey. Keep their original rows and check their scope as well.
        using var parsed = JsonDocument.Parse(key);
        var value = parsed.RootElement;
        var source = (ImportSourceType)value.GetProperty("SourceType").GetInt32();
        var shop = value.GetProperty("Shop").GetString(); var currency = value.GetProperty("Currency").GetString(); var id = value.GetProperty("Id").GetString();
        var legacy = await db.ImportLines.Join(db.ImportBatches, line => line.ImportBatchId, batch => batch.Id, (line, batch) => new { line, batch })
            .AnyAsync(x => x.line.SourceKey == null && x.line.Status == ImportLineStatus.Valid && x.line.ExternalId == id && x.batch.SourceType == source && x.batch.ShopCode == shop && x.batch.Currency == currency, cancellationToken);
        return legacy || source == ImportSourceType.MarketplaceStatement && await db.Set<FinanceFailedDeliveryFee>().AnyAsync(f => f.ShopCode == shop && f.Currency == currency && f.SourceFeeId == id && f.JournalEntryId != null, cancellationToken);
    }
    public Task<ImportBatch?> FindByChecksumAsync(string checksum, CancellationToken cancellationToken) => db.ImportBatches.Include(batch => batch.Lines).SingleOrDefaultAsync(batch => batch.Checksum == checksum, cancellationToken);
    public Task<ImportBatch?> FindByIdAsync(Guid id, CancellationToken cancellationToken) => db.ImportBatches.Include(batch => batch.Lines).SingleOrDefaultAsync(batch => batch.Id == id, cancellationToken);
    public async Task AddAsync(ImportBatch batch, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await RequireSourceModeAsync(batch.ShopCode, batch.SourceMode, cancellationToken);
        foreach (var line in batch.Lines.Where(l => l.Status == ImportLineStatus.Valid))
        {
            if (await db.Set<FinanceFailedDeliveryFee>().AnyAsync(f => f.ShopCode == batch.ShopCode && f.SourceFeeId == line.ExternalId && f.JournalEntryId != null, cancellationToken)) throw new InvalidOperationException("Phí giao thất bại đã ghi journal. Cần liên kết/điều chỉnh nguồn thay vì nhập lại phí.");
        }
        db.ImportBatches.Add(batch); await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<ImportBatch>> GetAllAsync(CancellationToken cancellationToken)
    {
        var batches = await db.ImportBatches.Include(batch => batch.Lines).ToListAsync(cancellationToken);
        return batches.OrderByDescending(batch => batch.ImportedAt).ToList();
    }
}
