using Finance.Application.Reconciliation;
using Finance.Domain.Reconciliation;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfReconciliationRepository(FinanceDbContext db) : IReconciliationRepository
{
    public Task<Payout?> FindPayoutAsync(Guid id, CancellationToken cancellationToken) => db.Payouts.FindAsync([id], cancellationToken).AsTask();
    public Task<Payout?> FindPayoutByExternalIdAsync(string externalId, string shopCode, CancellationToken cancellationToken) => db.Payouts.SingleOrDefaultAsync(value => value.ExternalId == externalId && value.ShopCode == shopCode, cancellationToken);
    public Task<BankTransaction?> FindBankTransactionAsync(Guid id, CancellationToken cancellationToken) => db.BankTransactions.FindAsync([id], cancellationToken).AsTask();
    public Task<BankTransaction?> FindBankTransactionByExternalIdAsync(string externalId, string shopCode, CancellationToken cancellationToken) => db.BankTransactions.SingleOrDefaultAsync(value => value.ExternalId == externalId && value.ShopCode == shopCode, cancellationToken);
    public Task<ReconciliationCase?> FindCaseAsync(Guid id, CancellationToken cancellationToken) => db.ReconciliationCases.FindAsync([id], cancellationToken).AsTask();
    public async Task AddPayoutAsync(Payout payout, CancellationToken cancellationToken) { db.Payouts.Add(payout); await db.SaveChangesAsync(cancellationToken); }
    public async Task AddBankTransactionAsync(BankTransaction transaction, CancellationToken cancellationToken) { db.BankTransactions.Add(transaction); await db.SaveChangesAsync(cancellationToken); }
    public async Task AddCaseAsync(ReconciliationCase reconciliationCase, CancellationToken cancellationToken) { db.ReconciliationCases.Add(reconciliationCase); await db.SaveChangesAsync(cancellationToken); }
    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<Payout>> GetPayoutsAsync(CancellationToken cancellationToken) => await db.Payouts.OrderByDescending(value => value.PayoutOn).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<BankTransaction>> GetBankTransactionsAsync(CancellationToken cancellationToken) => await db.BankTransactions.OrderByDescending(value => value.CreditedOn).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<ReconciliationCase>> GetCasesAsync(CancellationToken cancellationToken) => await db.ReconciliationCases.ToListAsync(cancellationToken);
}
