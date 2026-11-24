using Finance.Domain.Reconciliation;

namespace Finance.Application.Reconciliation;

public sealed record PayoutInput(string ExternalId, string ShopCode, string Currency, decimal Amount, DateOnly PayoutOn);
public sealed record BankTransactionInput(string ExternalId, string ShopCode, string Currency, decimal Amount, DateOnly CreditedOn);
public sealed record CreateReconciliationCommand(Guid PayoutId, Guid BankTransactionId, decimal Tolerance, string Evidence, string ProposedBy);
public sealed record ReconciliationResult(Guid Id, string Status, decimal Difference, decimal Tolerance, string? ReviewedBy);

public interface IReconciliationRepository
{
    Task<Payout?> FindPayoutAsync(Guid id, CancellationToken cancellationToken);
    Task<Payout?> FindPayoutByExternalIdAsync(string externalId, string shopCode, CancellationToken cancellationToken);
    Task<BankTransaction?> FindBankTransactionAsync(Guid id, CancellationToken cancellationToken);
    Task<BankTransaction?> FindBankTransactionByExternalIdAsync(string externalId, string shopCode, CancellationToken cancellationToken);
    Task<ReconciliationCase?> FindCaseAsync(Guid id, CancellationToken cancellationToken);
    Task AddPayoutAsync(Payout payout, CancellationToken cancellationToken);
    Task AddBankTransactionAsync(BankTransaction transaction, CancellationToken cancellationToken);
    Task AddCaseAsync(ReconciliationCase reconciliationCase, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<Payout>> GetPayoutsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BankTransaction>> GetBankTransactionsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ReconciliationCase>> GetCasesAsync(CancellationToken cancellationToken);
}
