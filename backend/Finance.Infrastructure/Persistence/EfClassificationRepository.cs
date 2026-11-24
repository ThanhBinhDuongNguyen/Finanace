using Finance.Application.Classification;
using Finance.Domain.Classification;
using Finance.Domain.Imports;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfClassificationRepository(FinanceDbContext db) : IClassificationRepository
{
    public Task<ClassificationMapping?> FindMappingAsync(ImportSourceType sourceType, string sourceBusinessType, DateOnly effectiveOn, CancellationToken cancellationToken) => db.ClassificationMappings.Where(value => value.SourceType == sourceType && value.SourceBusinessType == sourceBusinessType && value.EffectiveFrom <= effectiveOn && (value.EffectiveTo == null || value.EffectiveTo >= effectiveOn)).OrderByDescending(value => value.Version).FirstOrDefaultAsync(cancellationToken);
    public Task<bool> HasClassificationAsync(Guid importLineId, CancellationToken cancellationToken) => db.ClassifiedTransactions.AnyAsync(value => value.ImportLineId == importLineId, cancellationToken);
    public async Task AddMappingAsync(ClassificationMapping mapping, CancellationToken cancellationToken) { db.ClassificationMappings.Add(mapping); await db.SaveChangesAsync(cancellationToken); }
    public async Task AddTransactionsAsync(IEnumerable<ClassifiedTransaction> transactions, CancellationToken cancellationToken) { db.ClassifiedTransactions.AddRange(transactions); await db.SaveChangesAsync(cancellationToken); }
    public async Task<IReadOnlyList<ClassificationMapping>> GetMappingsAsync(CancellationToken cancellationToken) => await db.ClassificationMappings.OrderByDescending(value => value.Version).ToListAsync(cancellationToken);
    public async Task<IReadOnlyList<ClassifiedTransaction>> GetTransactionsAsync(CancellationToken cancellationToken) => await db.ClassifiedTransactions.ToListAsync(cancellationToken);
}
