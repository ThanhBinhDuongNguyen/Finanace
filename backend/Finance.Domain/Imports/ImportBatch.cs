namespace Finance.Domain.Imports;

public enum ImportSourceType { MarketplaceStatement, BankStatement }
public enum ImportBatchStatus { Committed, Duplicate }
public enum ImportLineStatus { Valid, Quarantined }

public sealed class ImportBatch
{
    public string SourceMode { get; init; } = "Local";
    public Guid Id { get; init; } = Guid.NewGuid();
    public ImportSourceType SourceType { get; init; }
    public string ShopCode { get; init; } = string.Empty;
    public DateOnly Period { get; init; }
    public string Currency { get; init; } = string.Empty;
    public string Checksum { get; init; } = string.Empty;
    public string ImportedBy { get; init; } = string.Empty;
    public DateTimeOffset ImportedAt { get; init; } = DateTimeOffset.UtcNow;
    public ImportBatchStatus Status { get; init; } = ImportBatchStatus.Committed;
    public List<ImportLine> Lines { get; init; } = [];
}

public sealed class ImportLine
{
    public string? SourceKey { get; init; }
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ImportBatchId { get; init; }
    public string ExternalId { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateOnly OccurredOn { get; init; }
    public string BusinessType { get; init; } = string.Empty;
    public string RawData { get; init; } = string.Empty;
    public ImportLineStatus Status { get; init; }
    public string? ValidationError { get; init; }
}
