namespace Finance.Domain.Ledger;

public enum JournalStatus { Posted, Reversed }

public sealed class JournalEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid? ReversesJournalEntryId { get; init; }
    public string ReferenceType { get; init; } = string.Empty;
    public string ReferenceId { get; init; } = string.Empty;
    public string ShopCode { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public DateOnly Period { get; init; }
    public string PostedBy { get; init; } = string.Empty;
    public JournalStatus Status { get; set; } = JournalStatus.Posted;
    public DateTimeOffset PostedAt { get; init; } = DateTimeOffset.UtcNow;
    public List<JournalLine> Lines { get; init; } = [];
}

public sealed class JournalLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid JournalEntryId { get; init; }
    public string AccountCode { get; init; } = string.Empty;
    public decimal Debit { get; init; }
    public decimal Credit { get; init; }
}

public sealed class ClosedPeriod
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = string.Empty;
    public DateOnly Period { get; init; }
    public string ClosedBy { get; init; } = string.Empty;
    public DateTimeOffset ClosedAt { get; init; } = DateTimeOffset.UtcNow;
}
