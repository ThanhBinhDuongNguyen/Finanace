using Finance.Domain.Exceptions;

namespace Finance.Application.Exceptions;

public sealed record CreateExceptionCommand(string ReferenceType, string ReferenceId, string ShopCode, string ReasonCode, decimal? Amount, string Owner, DateOnly DueOn, string Evidence, string CreatedBy);
public sealed record ExceptionActionCommand(string Actor, string Note, string Evidence);
public sealed record ExceptionResult(Guid Id, string Status, string Owner, DateOnly DueOn, int ActivityCount);

public interface IFinanceExceptionRepository
{
    Task<FinanceException?> FindAsync(Guid id, CancellationToken cancellationToken);
    Task AddAsync(FinanceException value, CancellationToken cancellationToken);
    Task SaveAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<FinanceException>> GetAllAsync(CancellationToken cancellationToken);
}
