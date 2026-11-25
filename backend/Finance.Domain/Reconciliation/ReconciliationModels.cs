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
    public string Reason { get; init; } = "";
    public List<StatementAllocation> StatementAllocations { get; init; } = [];
    public List<BankAllocation> BankAllocations { get; init; } = [];
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

public sealed class StatementAllocation
{
    public Guid? OrderLineId { get; init; }
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ReconciliationCaseId { get; init; }
    public Guid ImportLineId { get; init; }
    public decimal Amount { get; init; }
}
public sealed class BankAllocation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ReconciliationCaseId { get; init; }
    public Guid BankTransactionId { get; init; }
    public decimal Amount { get; init; }
}
public sealed class TolerancePolicy
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Currency { get; init; } = "";
    public decimal Amount { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly EffectiveTo { get; init; }
    public string ApprovedBy { get; init; } = "";
    public string Evidence { get; init; } = "";
}
