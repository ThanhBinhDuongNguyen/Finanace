using Finance.Domain.Classification;
using Finance.Domain.Imports;

namespace Finance.Application.Classification;

public sealed record CreateMappingCommand(ImportSourceType SourceType, string SourceBusinessType, string TargetCategory, int Version, DateOnly EffectiveFrom, DateOnly? EffectiveTo, string CreatedBy);
public sealed record ClassifyBatchCommand(DateOnly EffectiveOn, string Actor);
public sealed record ClassificationResult(int Classified, int Unmapped, int Skipped);

public interface IClassificationRepository
{
    Task<T> TransactionAsync<T>(Func<Task<T>> action, CancellationToken ct);
    Task<ClassificationMapping?> FindMappingAsync(ImportSourceType sourceType, string sourceBusinessType, DateOnly effectiveOn, CancellationToken cancellationToken);
    Task<bool> HasClassificationAsync(Guid importLineId, CancellationToken cancellationToken);
    Task AddMappingAsync(ClassificationMapping mapping, CancellationToken cancellationToken);
    Task AddTransactionsAsync(IEnumerable<ClassifiedTransaction> transactions, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClassificationMapping>> GetMappingsAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<ClassifiedTransaction>> GetTransactionsAsync(CancellationToken cancellationToken);
}
