using Finance.Domain.Imports;
using Finance.Application.Security;
using System.Text.Json;
using System.Globalization;

namespace Finance.Application.Imports;

public sealed class ImportBatchService(IImportBatchRepository repository, ICurrentActor actor)
{
    private static string Canonical(ImportLineInput line) => JsonSerializer.Serialize(new { Id = line.ExternalId?.Trim(), Amount = line.Amount.ToString("G29", CultureInfo.InvariantCulture), line.OccurredOn, Type = line.BusinessType?.Trim(), line.RawData });

    public async Task<ImportBatch> PreviewAsync(CreateImportBatchCommand command, CancellationToken cancellationToken)
    {
        if (!Enum.IsDefined(command.SourceType) || command.Lines is null || command.Lines.Count is 0 or > 10000) throw new ArgumentException("Batch phải có 1–10.000 dòng và loại nguồn hợp lệ.");
        var shop = FinanceRules.Scope(command.ShopCode);
        actor.RequireShop(shop);
        if (command.SourceMode == "Connected") actor.RequireRole("Integration");
        await repository.RequireSourceModeAsync(shop, command.SourceMode, cancellationToken);
        var currency = FinanceRules.Scope(command.Currency);
        var period = FinanceRules.Month(command.Period);
        var batch = new ImportBatch { SourceMode = command.SourceMode, ShopCode = shop, Currency = currency, Period = period, SourceType = command.SourceType, ImportedBy = actor.UserName,
            Checksum = ImportFingerprint.Create(command.SourceType, shop, period, currency, command.Lines.Select(Canonical)) };
        var seen = new HashSet<string>();
        foreach (var input in command.Lines)
        {
            var line = CreateLine(batch.Id, input);
            var key = JsonSerializer.Serialize(new { command.SourceType, Shop = shop, Currency = currency, Id = input.ExternalId?.Trim() });
            var duplicate = !seen.Add(key) || await repository.SourceKeyExistsAsync(key, cancellationToken);
            var error = line.ValidationError ?? (input.OccurredOn == default ? "Thiếu ngày nghiệp vụ." : duplicate ? "Sự kiện nguồn đã tồn tại." : null);
            batch.Lines.Add(new ImportLine { ImportBatchId = batch.Id, ExternalId = line.ExternalId, Amount = line.Amount, BusinessType = line.BusinessType, OccurredOn = line.OccurredOn,
                RawData = line.RawData, SourceKey = error is null ? key : null, Status = error is null ? ImportLineStatus.Valid : ImportLineStatus.Quarantined, ValidationError = error });
        }
        return batch;
    }
    public async Task<ImportBatchResult> CreateAsync(CreateImportBatchCommand command, CancellationToken cancellationToken)
    {
        var preview = await PreviewAsync(command, cancellationToken);
        var checksum = preview.Checksum;
        var existing = await repository.FindByChecksumAsync(checksum, cancellationToken);
        if (existing is not null)
            return ToResult(existing, true);

        await repository.AddAsync(preview, cancellationToken);
        return ToResult(preview, false);
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
