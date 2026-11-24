using Finance.Domain.Exceptions;

namespace Finance.Application.Exceptions;

public sealed class ExceptionService(IFinanceExceptionRepository repository)
{
    public async Task<ExceptionResult> CreateAsync(CreateExceptionCommand command, CancellationToken cancellationToken)
    {
        var value = new FinanceException
        {
            ReferenceType = Required(command.ReferenceType, "Loại tham chiếu"),
            ReferenceId = Required(command.ReferenceId, "Mã tham chiếu"),
            ShopCode = Required(command.ShopCode, "Shop"),
            ReasonCode = Required(command.ReasonCode, "Mã lý do"),
            Amount = command.Amount,
            Owner = Required(command.Owner, "Owner"),
            DueOn = command.DueOn,
            Evidence = Required(command.Evidence, "Evidence"),
            CreatedBy = Required(command.CreatedBy, "Người tạo"),
        };
        value.Activities.Add(Activity(value.Id, "Created", value.CreatedBy, "Tạo ngoại lệ", value.Evidence));
        await repository.AddAsync(value, cancellationToken);
        return ToResult(value);
    }

    public async Task<ExceptionResult> RequestClosureAsync(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken)
    {
        var value = await GetAsync(id, cancellationToken);
        if (value.Status != FinanceExceptionStatus.Open) throw new ArgumentException("Chỉ ngoại lệ Open mới được yêu cầu đóng.");
        value.Status = FinanceExceptionStatus.PendingApproval;
        value.Activities.Add(Activity(value.Id, "ClosureRequested", Required(command.Actor, "Người yêu cầu"), Required(command.Note, "Kết quả xử lý"), Required(command.Evidence, "Evidence xác minh")));
        await repository.SaveAsync(cancellationToken);
        return ToResult(value);
    }

    public async Task<ExceptionResult> ApproveClosureAsync(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken)
    {
        var value = await GetAsync(id, cancellationToken);
        var reviewer = Required(command.Actor, "Người duyệt");
        if (value.Status != FinanceExceptionStatus.PendingApproval) throw new ArgumentException("Ngoại lệ chưa ở trạng thái chờ duyệt đóng.");
        if (string.Equals(value.CreatedBy, reviewer, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Người tạo không được tự duyệt đóng ngoại lệ.");
        value.Status = FinanceExceptionStatus.Closed;
        value.Activities.Add(Activity(value.Id, "ClosureApproved", reviewer, Required(command.Note, "Quyết định"), Required(command.Evidence, "Evidence duyệt")));
        await repository.SaveAsync(cancellationToken);
        return ToResult(value);
    }

    public async Task<ExceptionResult> ReopenAsync(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken)
    {
        var value = await GetAsync(id, cancellationToken);
        if (value.Status != FinanceExceptionStatus.Closed) throw new ArgumentException("Chỉ ngoại lệ Closed mới được mở lại.");
        value.Status = FinanceExceptionStatus.Open;
        value.Activities.Add(Activity(value.Id, "Reopened", Required(command.Actor, "Người mở lại"), Required(command.Note, "Lý do mở lại"), Required(command.Evidence, "Evidence mới")));
        await repository.SaveAsync(cancellationToken);
        return ToResult(value);
    }

    private async Task<FinanceException> GetAsync(Guid id, CancellationToken cancellationToken) => await repository.FindAsync(id, cancellationToken) ?? throw new ArgumentException("Không tìm thấy ngoại lệ.");
    private static ExceptionActivity Activity(Guid exceptionId, string action, string actor, string note, string evidence) => new() { FinanceExceptionId = exceptionId, Action = action, Actor = actor, Note = note, Evidence = evidence };
    private static ExceptionResult ToResult(FinanceException value) => new(value.Id, value.Status.ToString(), value.Owner, value.DueOn, value.Activities.Count);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} là bắt buộc.") : value.Trim();
}
