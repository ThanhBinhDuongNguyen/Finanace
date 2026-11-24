using Finance.Domain.Imports;

namespace Finance.Application.Imports;

public sealed record ImportLineInput(string? ExternalId, decimal Amount, DateOnly OccurredOn, string? BusinessType, string? RawData);
public sealed record CreateImportBatchCommand(ImportSourceType SourceType, string ShopCode, DateOnly Period, string Currency, string ImportedBy, IReadOnlyList<ImportLineInput> Lines);
public sealed record ImportBatchResult(Guid Id, string Status, string Checksum, int ValidLines, int QuarantinedLines, bool IsDuplicate);

public interface IImportBatchRepository
{
    Task<ImportBatch?> FindByChecksumAsync(string checksum, CancellationToken cancellationToken);
    Task<ImportBatch?> FindByIdAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(ImportBatch batch, CancellationToken cancellationToken);
    Task<IReadOnlyList<ImportBatch>> GetAllAsync(CancellationToken cancellationToken);
}
