using Finance.Application.Reconciliation;
using Finance.Domain.Reconciliation;
using Microsoft.EntityFrameworkCore;
using Finance.Domain.Imports;
using Finance.Domain.Extensions;

namespace Finance.Infrastructure.Persistence;

public sealed class EfReconciliationRepository(FinanceDbContext db) : IReconciliationRepository
{
    public Task<Payout?> FindPayoutAsync(Guid id, CancellationToken cancellationToken) => db.Payouts.FindAsync([id], cancellationToken).AsTask();
    public Task<Payout?> FindPayoutByExternalIdAsync(string externalId, string shopCode, CancellationToken cancellationToken) => db.Payouts.SingleOrDefaultAsync(value => value.ExternalId == externalId && value.ShopCode == shopCode, cancellationToken);
    public Task<BankTransaction?> FindBankTransactionAsync(Guid id, CancellationToken cancellationToken) => db.BankTransactions.FindAsync([id], cancellationToken).AsTask();
    public Task<BankTransaction?> FindBankTransactionByExternalIdAsync(string externalId, string shopCode, CancellationToken cancellationToken) => db.BankTransactions.SingleOrDefaultAsync(value => value.ExternalId == externalId && value.ShopCode == shopCode, cancellationToken);
    public Task<ReconciliationCase?> FindCaseAsync(Guid id, CancellationToken cancellationToken) => db.ReconciliationCases.Include(v => v.StatementAllocations).Include(v => v.BankAllocations).SingleOrDefaultAsync(v => v.Id == id, cancellationToken);
    public async Task AddPayoutAsync(Payout payout, CancellationToken cancellationToken) { db.Payouts.Add(payout); await db.SaveChangesAsync(cancellationToken); }
    public async Task AddBankTransactionAsync(BankTransaction transaction, CancellationToken cancellationToken) { db.BankTransactions.Add(transaction); await db.SaveChangesAsync(cancellationToken); }
    public async Task AddCaseAsync(ReconciliationCase value, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var payout = await db.Payouts.SingleAsync(p => p.Id == value.PayoutId, ct);
        if (await db.ReconciliationCases.AnyAsync(c => c.PayoutId == value.PayoutId, ct)) throw new InvalidOperationException("Payout đã có case đối soát.");
        if (value.BankAllocations.Select(b => b.BankTransactionId).Distinct().Count() != value.BankAllocations.Count || value.StatementAllocations.Select(a => a.ImportLineId).Distinct().Count() != value.StatementAllocations.Count) throw new ArgumentException("Allocation bị trùng trong request.");
        foreach (var allocation in value.BankAllocations)
        {
            var source = await db.BankTransactions.SingleAsync(b => b.Id == allocation.BankTransactionId, ct);
            var reserved = (await db.Set<BankAllocation>().Where(a => a.BankTransactionId == source.Id).ToListAsync(ct)).Sum(a => a.Amount);
            if (allocation.Amount <= 0 || allocation.Amount + reserved > source.Amount) throw new InvalidOperationException("Bank allocation vượt số dư chưa phân bổ.");
        }
        foreach (var allocation in value.StatementAllocations)
        {
            var line = await db.ImportLines.SingleOrDefaultAsync(l => l.Id == allocation.ImportLineId, ct) ?? throw new ArgumentException("Không tìm thấy dòng statement.");
            var batch = await db.ImportBatches.SingleAsync(b => b.Id == line.ImportBatchId, ct);
            var used = (await db.Set<StatementAllocation>().Where(a => a.ImportLineId == line.Id).ToListAsync(ct)).Sum(a => Math.Abs(a.Amount));
            if (line.Status != ImportLineStatus.Valid || batch.ShopCode != payout.ShopCode || batch.Currency != payout.Currency || batch.SourceType != ImportSourceType.MarketplaceStatement || Math.Sign(allocation.Amount) != Math.Sign(line.Amount) || Math.Abs(allocation.Amount) + used > Math.Abs(line.Amount)) throw new ArgumentException("Statement allocation không hợp lệ hoặc vượt nguồn.");
            if (allocation.OrderLineId is not null)
            {
                var order = await db.Set<FinanceOrderLine>().SingleOrDefaultAsync(o => o.Id == allocation.OrderLineId, ct) ?? throw new ArgumentException("Không tìm thấy order line.");
                var allocatedToOrder = (await db.Set<StatementAllocation>().Where(a => a.OrderLineId == order.Id && a.Amount > 0).ToListAsync(ct)).Sum(a => a.Amount);
                if (order.ShopCode != payout.ShopCode || order.Currency != payout.Currency || allocation.Amount > 0 && allocation.Amount + allocatedToOrder > order.NetAmount) throw new ArgumentException("Order allocation vượt doanh thu hoặc sai scope.");
            }
        }
        if (value.StatementAllocations.Count > 0 && value.StatementAllocations.Sum(a => a.Amount) != payout.Amount) throw new ArgumentException("Tổng statement allocation phải bằng payout.");
        db.ReconciliationCases.Add(value);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
    }
    public async Task<decimal> GetToleranceAsync(string currency, DateOnly on, CancellationToken ct) => (await db.Set<TolerancePolicy>().Where(p => p.Currency == currency && p.EffectiveFrom <= on && p.EffectiveTo >= on).ToListAsync(ct)).SingleOrDefault()?.Amount ?? 0;
    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<Payout>> GetPayoutsAsync(CancellationToken cancellationToken) => await db.Payouts.OrderByDescending(value => value.PayoutOn).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<BankTransaction>> GetBankTransactionsAsync(CancellationToken cancellationToken) => await db.BankTransactions.OrderByDescending(value => value.CreditedOn).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<ReconciliationCase>> GetCasesAsync(CancellationToken cancellationToken) => await db.ReconciliationCases.Include(v => v.StatementAllocations).Include(v => v.BankAllocations).ToListAsync(cancellationToken);
}
