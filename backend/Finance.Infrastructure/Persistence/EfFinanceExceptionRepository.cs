using Finance.Application.Exceptions;
using Finance.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Persistence;

public sealed class EfFinanceExceptionRepository(FinanceDbContext db) : IFinanceExceptionRepository
{
    public Task<FinanceException?> FindAsync(Guid id, CancellationToken cancellationToken) => db.FinanceExceptions.Include(value => value.Activities).SingleOrDefaultAsync(value => value.Id == id, cancellationToken);
    public async Task AddAsync(FinanceException value, CancellationToken cancellationToken) { db.FinanceExceptions.Add(value); await db.SaveChangesAsync(cancellationToken); }
    public Task SaveAsync(CancellationToken cancellationToken) => db.SaveChangesAsync(cancellationToken);
    public async Task<IReadOnlyList<FinanceException>> GetAllAsync(CancellationToken cancellationToken) => await db.FinanceExceptions.Include(value => value.Activities).ToListAsync(cancellationToken);
}
