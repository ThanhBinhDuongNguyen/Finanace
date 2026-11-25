namespace Finance.Domain.Ledger;

public sealed class PeriodReopenRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public DateOnly Period { get; init; }
    public string ProposedBy { get; init; } = "";
    public string Reason { get; init; } = "";
    public string Evidence { get; init; } = "";
    public string? ApprovedBy { get; set; }
    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;
}
