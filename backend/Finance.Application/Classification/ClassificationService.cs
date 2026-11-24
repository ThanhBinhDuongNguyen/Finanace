using Finance.Application.Exceptions;
using Finance.Application.Imports;
using Finance.Domain.Classification;
using Finance.Domain.Exceptions;
using Finance.Domain.Imports;

namespace Finance.Application.Classification;

public sealed class ClassificationService(IImportBatchRepository importRepository, IClassificationRepository classificationRepository, IFinanceExceptionRepository exceptionRepository)
{
    public async Task AddMappingAsync(CreateMappingCommand command, CancellationToken cancellationToken)
    {
        if (command.Version < 1) throw new ArgumentException("Version phải từ 1.");
        if (command.EffectiveTo is not null && command.EffectiveTo < command.EffectiveFrom) throw new ArgumentException("Ngày kết thúc không thể trước ngày hiệu lực.");
        await classificationRepository.AddMappingAsync(new ClassificationMapping
        {
            SourceType = command.SourceType,
            SourceBusinessType = Required(command.SourceBusinessType, "Loại nghiệp vụ nguồn"),
            TargetCategory = Required(command.TargetCategory, "Nhóm đích"),
            Version = command.Version,
            EffectiveFrom = command.EffectiveFrom,
            EffectiveTo = command.EffectiveTo,
            CreatedBy = Required(command.CreatedBy, "Người tạo"),
        }, cancellationToken);
    }

    public async Task<ClassificationResult> ClassifyBatchAsync(Guid batchId, ClassifyBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await importRepository.FindByIdAsync(batchId, cancellationToken) ?? throw new ArgumentException("Không tìm thấy import batch.");
        var actor = Required(command.Actor, "Người phân loại");
        var transactions = new List<ClassifiedTransaction>();
        var exceptions = new List<FinanceException>();
        var classified = 0; var unmapped = 0; var skipped = 0;

        foreach (var line in batch.Lines)
        {
            if (line.Status == ImportLineStatus.Quarantined || await classificationRepository.HasClassificationAsync(line.Id, cancellationToken)) { skipped++; continue; }
            var mapping = await classificationRepository.FindMappingAsync(batch.SourceType, line.BusinessType, command.EffectiveOn, cancellationToken);
            var isMapped = mapping is not null;
            transactions.Add(new ClassifiedTransaction { ImportBatchId = batch.Id, ImportLineId = line.Id, ClassificationMappingId = mapping?.Id, Category = mapping?.TargetCategory ?? "Unmapped", Status = isMapped ? ClassificationStatus.Classified : ClassificationStatus.Unmapped, ClassifiedBy = actor });
            if (isMapped) classified++;
            else
            {
                unmapped++;
                var exceptionId = Guid.NewGuid();
                exceptions.Add(new FinanceException
                {
                    Id = exceptionId,
                    ReferenceType = "ImportLine",
                    ReferenceId = line.Id.ToString(),
                    ShopCode = batch.ShopCode,
                    ReasonCode = "UNMAPPED_SOURCE_TYPE",
                    Amount = line.Amount,
                    Owner = "finance.mapping",
                    DueOn = command.EffectiveOn.AddDays(2),
                    Evidence = line.RawData,
                    CreatedBy = actor,
                    Activities = [new ExceptionActivity { FinanceExceptionId = exceptionId, Action = "Created", Actor = actor, Note = "Loại nghiệp vụ nguồn chưa có mapping.", Evidence = line.RawData }],
                });
            }
        }
        if (transactions.Count > 0) await classificationRepository.AddTransactionsAsync(transactions, cancellationToken);
        foreach (var value in exceptions) await exceptionRepository.AddAsync(value, cancellationToken);
        return new ClassificationResult(classified, unmapped, skipped);
    }

    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} là bắt buộc.") : value.Trim();
}
