using Finance.Domain.Imports;

namespace Finance.Application.Imports;

public sealed class ImportBatchService(IImportBatchRepository repository)
{
    public async Task<ImportBatchResult> CreateAsync(CreateImportBatchCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.ShopCode) || string.IsNullOrWhiteSpace(command.Currency) || command.Lines.Count == 0)
            throw new ArgumentException("ShopCode, Currency và ít nhất một dòng import là bắt buộc.");

        var checksum = ImportFingerprint.Create(command.SourceType, command.ShopCode, command.Period, command.Currency, command.Lines.Select(line => line.RawData ?? string.Empty));
        var existing = await repository.FindByChecksumAsync(checksum, cancellationToken);
        if (existing is not null)
            return ToResult(existing, true);

        var batchId = Guid.NewGuid();
        var lines = command.Lines.Select(line => CreateLine(batchId, line)).ToList();
        var batch = new ImportBatch
        {
            Id = batchId,
            SourceType = command.SourceType,
            ShopCode = command.ShopCode.Trim(),
            Period = command.Period,
            Currency = command.Currency.Trim().ToUpperInvariant(),
            ImportedBy = string.IsNullOrWhiteSpace(command.ImportedBy) ? "system" : command.ImportedBy.Trim(),
            Checksum = checksum,
            Lines = lines,
        };

        await repository.AddAsync(batch, cancellationToken);
        return ToResult(batch, false);
    }

    private static ImportLine CreateLine(Guid batchId, ImportLineInput input)
    {
        var error = string.IsNullOrWhiteSpace(input.ExternalId) ? "Thiếu mã giao dịch nguồn."
            : string.IsNullOrWhiteSpace(input.BusinessType) ? "Thiếu loại nghiệp vụ."
            : input.Amount == 0 ? "Số tiền phải khác 0." : null;
        return new ImportLine
        {
            ImportBatchId = batchId,
            ExternalId = input.ExternalId?.Trim() ?? string.Empty,
            Amount = input.Amount,
            OccurredOn = input.OccurredOn,
            BusinessType = input.BusinessType?.Trim() ?? string.Empty,
            RawData = input.RawData ?? string.Empty,
            Status = error is null ? ImportLineStatus.Valid : ImportLineStatus.Quarantined,
            ValidationError = error,
        };
    }

    private static ImportBatchResult ToResult(ImportBatch batch, bool isDuplicate) => new(batch.Id, isDuplicate ? ImportBatchStatus.Duplicate.ToString() : batch.Status.ToString(), batch.Checksum, batch.Lines.Count(line => line.Status == ImportLineStatus.Valid), batch.Lines.Count(line => line.Status == ImportLineStatus.Quarantined), isDuplicate);
}
