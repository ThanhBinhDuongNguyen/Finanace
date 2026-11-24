using Finance.Domain.Ledger;

namespace Finance.Application.Ledger;

public sealed record JournalLineInput(string AccountCode, decimal Debit, decimal Credit);
public sealed record CreateJournalCommand(string ReferenceType, string ReferenceId, string ShopCode, string Currency, DateOnly Period, string PostedBy, IReadOnlyList<JournalLineInput> Lines);
public sealed record ReverseJournalCommand(DateOnly Period, string PostedBy);
public sealed record ClosePeriodCommand(string ShopCode, DateOnly Period, string ClosedBy);
public sealed record JournalResult(Guid Id, string Status, decimal Debit, decimal Credit, Guid? ReversesJournalEntryId);

public interface ILedgerRepository
{
    Task<bool> IsPeriodClosedAsync(string shopCode, DateOnly period, CancellationToken cancellationToken);
    Task<JournalEntry?> FindJournalAsync(Guid id, CancellationToken cancellationToken);
    Task AddJournalAsync(JournalEntry entry, CancellationToken cancellationToken);
    Task AddClosedPeriodAsync(ClosedPeriod period, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<JournalEntry>> GetJournalsAsync(CancellationToken cancellationToken);
}
