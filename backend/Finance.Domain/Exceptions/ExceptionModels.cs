namespace Finance.Domain.Exceptions;

public enum FinanceExceptionStatus { Open, PendingApproval, Closed, WaitingExternal }

public sealed class FinanceException
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ReferenceType { get; init; } = string.Empty;
    public string ReferenceId { get; init; } = string.Empty;
    public string ShopCode { get; init; } = string.Empty;
    public string ReasonCode { get; init; } = string.Empty;
    public decimal? Amount { get; init; }
    public string Owner { get; init; } = string.Empty;
    public DateOnly DueOn { get; init; }
    public string Evidence { get; init; } = string.Empty;
    public string CreatedBy { get; init; } = string.Empty;
    public FinanceExceptionStatus Status { get; set; } = FinanceExceptionStatus.Open;
    public List<ExceptionActivity> Activities { get; init; } = [];
}

public sealed class ExceptionActivity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid FinanceExceptionId { get; init; }
    public string Action { get; init; } = string.Empty;
    public string Actor { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public string Evidence { get; init; } = string.Empty;
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}
