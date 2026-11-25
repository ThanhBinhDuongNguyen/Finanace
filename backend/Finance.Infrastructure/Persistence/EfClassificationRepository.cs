using Finance.Application.Classification;
using Finance.Domain.Classification;
using Finance.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfClassificationRepository(FinanceDbContext db) : IClassificationRepository
{
    public async Task<T> TransactionAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var result = await action(); await transaction.CommitAsync(ct); return result;
    }
    public Task<ClassificationMapping?> FindMappingAsync(ImportSourceType sourceType, string sourceBusinessType, DateOnly effectiveOn, CancellationToken cancellationToken) => db.ClassificationMappings.Where(value => value.SourceType == sourceType && value.SourceBusinessType == sourceBusinessType && value.EffectiveFrom <= effectiveOn && (value.EffectiveTo == null || value.EffectiveTo >= effectiveOn)).OrderByDescending(value => value.Version).FirstOrDefaultAsync(cancellationToken);
    public Task<bool> HasClassificationAsync(Guid importLineId, CancellationToken cancellationToken) => db.ClassifiedTransactions.AnyAsync(value => value.ImportLineId == importLineId && value.Status == ClassificationStatus.Classified, cancellationToken);
    public async Task AddMappingAsync(ClassificationMapping mapping, CancellationToken cancellationToken)
    {
        var mappings = await db.ClassificationMappings.Where(v => v.SourceType == mapping.SourceType && v.SourceBusinessType == mapping.SourceBusinessType).ToListAsync(cancellationToken);
        if (mappings.Any(v => v.Version == mapping.Version || v.EffectiveFrom <= (mapping.EffectiveTo ?? DateOnly.MaxValue) && (v.EffectiveTo ?? DateOnly.MaxValue) >= mapping.EffectiveFrom)) throw new InvalidOperationException("Mapping trùng version hoặc chồng ngày hiệu lực.");
        db.ClassificationMappings.Add(mapping); await db.SaveChangesAsync(cancellationToken);
    }
    public async Task AddTransactionsAsync(IEnumerable<ClassifiedTransaction> transactions, CancellationToken cancellationToken)
    {
        foreach (var item in transactions)
        {
            var existing = await db.ClassifiedTransactions.SingleOrDefaultAsync(v => v.ImportLineId == item.ImportLineId, cancellationToken);
            if (existing is null) db.ClassifiedTransactions.Add(item);
            else { existing.Status = item.Status; existing.Category = item.Category; existing.ClassificationMappingId = item.ClassificationMappingId; }
        }
        await db.SaveChangesAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<ClassificationMapping>> GetMappingsAsync(CancellationToken cancellationToken) => await db.ClassificationMappings.OrderByDescending(value => value.Version).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<ClassifiedTransaction>> GetTransactionsAsync(CancellationToken cancellationToken) => await db.ClassifiedTransactions.ToListAsync(cancellationToken);
}
