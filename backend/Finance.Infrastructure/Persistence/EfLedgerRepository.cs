using Finance.Application.Ledger;
using Finance.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfLedgerRepository(FinanceDbContext db) : ILedgerRepository
{
    public Task<JournalEntry?> FindSourceAsync(string shop, string referenceType, string referenceId, CancellationToken ct) => db.JournalEntries.Include(value => value.Lines).SingleOrDefaultAsync(value => value.ShopCode == shop && value.ReferenceType == referenceType && value.ReferenceId == referenceId, ct);
    public Task<bool> IsPeriodClosedAsync(string shopCode, DateOnly period, CancellationToken cancellationToken) { var month = new DateOnly(period.Year, period.Month, 1); var end = month.AddMonths(1); return db.ClosedPeriods.AnyAsync(value => value.ShopCode == shopCode && value.Period >= month && value.Period < end, cancellationToken); }
    public Task<JournalEntry?> FindJournalAsync(Guid id, CancellationToken cancellationToken) => db.JournalEntries.Include(value => value.Lines).SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
    public async Task AddJournalAsync(JournalEntry entry, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (await IsPeriodClosedAsync(entry.ShopCode, entry.Period, cancellationToken)) throw new ArgumentException("Kỳ đã khóa trước khi ghi journal.");
        db.JournalEntries.Add(entry); await db.SaveChangesAsync(cancellationToken); await transaction.CommitAsync(cancellationToken);
    }
    public async Task AddClosedPeriodAsync(ClosedPeriod period, CancellationToken cancellationToken) { db.ClosedPeriods.Add(period); await db.SaveChangesAsync(cancellationToken); }
    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<JournalEntry>> GetJournalsAsync(CancellationToken cancellationToken) => await db.JournalEntries.Include(value => value.Lines).ToListAsync(cancellationToken);
}
