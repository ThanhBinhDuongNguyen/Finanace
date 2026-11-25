using Finance.Domain.Imports;

namespace Finance.Domain.Classification;

public enum ClassificationStatus { Classified, Unmapped, Skipped }

public sealed class ClassificationMapping
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public ImportSourceType SourceType { get; init; }
    public string SourceBusinessType { get; init; } = string.Empty;
    public string TargetCategory { get; init; } = string.Empty;
    public int Version { get; init; }
    public DateOnly EffectiveFrom { get; init; }
    public DateOnly? EffectiveTo { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
}

public sealed class ClassifiedTransaction
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid ImportBatchId { get; init; }
    public Guid ImportLineId { get; init; }
    public Guid? ClassificationMappingId { get; set; }
    public string Category { get; set; } = string.Empty;
    public ClassificationStatus Status { get; set; }
    public string ClassifiedBy { get; init; } = string.Empty;
    public DateTimeOffset ClassifiedAt { get; init; } = DateTimeOffset.UtcNow;
}
