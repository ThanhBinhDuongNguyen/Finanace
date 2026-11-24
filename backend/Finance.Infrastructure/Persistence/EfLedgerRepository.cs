using Finance.Application.Ledger;
using Finance.Domain.Ledger;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfLedgerRepository(FinanceDbContext db) : ILedgerRepository
{
    public Task<bool> IsPeriodClosedAsync(string shopCode, DateOnly period, CancellationToken cancellationToken) => db.ClosedPeriods.AnyAsync(value => value.ShopCode == shopCode && value.Period == period, cancellationToken);
    public Task<JournalEntry?> FindJournalAsync(Guid id, CancellationToken cancellationToken) => db.JournalEntries.Include(value => value.Lines).SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
    public async Task AddJournalAsync(JournalEntry entry, CancellationToken cancellationToken) { db.JournalEntries.Add(entry); await db.SaveChangesAsync(cancellationToken); }
    public async Task AddClosedPeriodAsync(ClosedPeriod period, CancellationToken cancellationToken) { db.ClosedPeriods.Add(period); await db.SaveChangesAsync(cancellationToken); }
    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<JournalEntry>> GetJournalsAsync(CancellationToken cancellationToken) => await db.JournalEntries.Include(value => value.Lines).ToListAsync(cancellationToken);
}
