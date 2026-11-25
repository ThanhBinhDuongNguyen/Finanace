using Finance.Domain.Exceptions;
using Finance.Application.Security;

namespace Finance.Application.Exceptions;

public sealed class ExceptionService(IFinanceExceptionRepository repository, ICurrentActor actor)
{
    public async Task<ExceptionResult> CreateAsync(CreateExceptionCommand command, CancellationToken cancellationToken)
    {
        actor.RequireShop(command.ShopCode);
        command = command with { CreatedBy = actor.UserName, ShopCode = FinanceRules.Scope(command.ShopCode) };
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
        if (value.Status is not (FinanceExceptionStatus.Open or FinanceExceptionStatus.WaitingExternal)) throw new ArgumentException("Ngoại lệ không ở trạng thái xử lý.");
        if (command.Outcome is not ("Verified" or "AcceptVariance")) throw new ArgumentException("Phải có kết quả Verified hoặc đề xuất AcceptVariance. Ticket gửi đi chưa phải kết quả xử lý.");
        value.Status = FinanceExceptionStatus.PendingApproval;
        value.Activities.Add(Activity(value.Id, "ClosureRequested", actor.UserName, command.Outcome + ": " + Required(command.Note, "Kết quả xử lý"), Required(command.Evidence, "Evidence xác minh")));
        await repository.SaveAsync(cancellationToken);
        return ToResult(value);
    }

    public async Task<ExceptionResult> ApproveClosureAsync(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken)
    {
        var value = await GetAsync(id, cancellationToken);
        actor.RequireRole("Checker");
        actor.RequireApproval(value.Amount ?? 0, "ExceptionClosure");
        var reviewer = actor.UserName;
        if (value.Status != FinanceExceptionStatus.PendingApproval) throw new ArgumentException("Ngoại lệ chưa ở trạng thái chờ duyệt đóng.");
        if (string.Equals(value.CreatedBy, reviewer, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Người tạo không được tự duyệt đóng ngoại lệ.");
        if (value.Activities.LastOrDefault(a => a.Action == "ClosureRequested")?.Actor == reviewer) throw new ArgumentException("Người đề xuất đóng không được tự duyệt.");
        value.Status = FinanceExceptionStatus.Closed;
        value.Activities.Add(Activity(value.Id, "ClosureApproved", reviewer, Required(command.Note, "Quyết định"), Required(command.Evidence, "Evidence duyệt")));
        await repository.SaveAsync(cancellationToken);
        return ToResult(value);
    }

    public async Task<ExceptionResult> ReopenAsync(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken)
    {
        var value = await GetAsync(id, cancellationToken);
        if (value.Status != FinanceExceptionStatus.Closed) throw new ArgumentException("Chỉ ngoại lệ Closed mới được mở lại.");
        if (value.Activities.Any(a => a.Evidence == Required(command.Evidence, "Evidence mới"))) throw new ArgumentException("Mở lại cần bằng chứng mới chưa có trong hồ sơ.");
        value.Status = FinanceExceptionStatus.Open;
        value.Activities.Add(Activity(value.Id, "Reopened", actor.UserName, Required(command.Note, "Lý do mở lại"), Required(command.Evidence, "Evidence mới")));
        await repository.SaveAsync(cancellationToken);
        return ToResult(value);
    }

    public async Task<ExceptionResult> WaitExternalAsync(Guid id, ExceptionActionCommand command, CancellationToken ct)
    {
        var value = await GetAsync(id, ct);
        if (value.Status != FinanceExceptionStatus.Open) throw new ArgumentException("Chỉ ngoại lệ Open được chuyển sang WaitingExternal.");
        value.Activities.Add(Activity(id, "WaitingExternal", actor.UserName, Required(command.Note, "Ticket/lý do"), Required(command.Evidence, "Evidence")));
        value.Status = FinanceExceptionStatus.WaitingExternal;
        await repository.SaveAsync(ct);
        return ToResult(value);
    }

    private async Task<FinanceException> GetAsync(Guid id, CancellationToken cancellationToken) => await repository.FindAsync(id, cancellationToken) ?? throw new ArgumentException("Không tìm thấy ngoại lệ.");
    private static ExceptionActivity Activity(Guid exceptionId, string action, string actor, string note, string evidence) => new() { FinanceExceptionId = exceptionId, Action = action, Actor = actor, Note = note, Evidence = evidence };
    private static ExceptionResult ToResult(FinanceException value) => new(value.Id, value.Status.ToString(), value.Owner, value.DueOn, value.Activities.Count);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} là bắt buộc.") : value.Trim();
}
