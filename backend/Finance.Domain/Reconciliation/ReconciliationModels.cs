namespace Finance.Domain.Reconciliation;

public enum ReconciliationStatus { Candidate, FullyMatched, Disputed }

public sealed class Payout
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ExternalId { get; init; } = string.Empty;
    public string ShopCode { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly PayoutOn { get; init; }
}

public sealed class BankTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ExternalId { get; init; } = string.Empty;
    public string ShopCode { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly CreditedOn { get; init; }
}

public sealed class ReconciliationCase
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid PayoutId { get; init; }
    public Guid BankTransactionId { get; init; }
    public decimal Difference { get; init; }
    public decimal Tolerance { get; init; }
    public ReconciliationStatus Status { get; set; }
    public string Evidence { get; init; } = string.Empty;
    public string ProposedBy { get; init; } = string.Empty;
    public string? ReviewedBy { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
