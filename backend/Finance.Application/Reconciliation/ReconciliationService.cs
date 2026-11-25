using Finance.Domain.Reconciliation;
using Finance.Application.Security;

namespace Finance.Application.Reconciliation;

public sealed class ReconciliationService(IReconciliationRepository repository, ICurrentActor actor)
{
    public async Task AddPayoutAsync(PayoutInput input, CancellationToken cancellationToken)
    {
        if (input.PayoutOn == default) throw new ArgumentException("Thiếu ngày payout.");
        var externalId = Required(input.ExternalId, "Mã payout");
        var shopCode = FinanceRules.Scope(input.ShopCode);
        actor.RequireShop(shopCode);
        var existing = await repository.FindPayoutByExternalIdAsync(externalId, shopCode, cancellationToken);
        if (existing is not null && (existing.Amount != input.Amount || existing.Currency != FinanceRules.Scope(input.Currency) || existing.PayoutOn != input.PayoutOn)) throw new InvalidOperationException("Payout cùng mã có dữ liệu khác.");
        if (existing is null)
            await repository.AddPayoutAsync(new Payout { ExternalId = externalId, ShopCode = shopCode, Currency = Required(input.Currency, "Currency").ToUpperInvariant(), Amount = NonZero(input.Amount), PayoutOn = input.PayoutOn }, cancellationToken);
    }

    public async Task AddBankTransactionAsync(BankTransactionInput input, CancellationToken cancellationToken)
    {
        if (input.CreditedOn == default) throw new ArgumentException("Thiếu ngày bank ghi có.");
        var externalId = Required(input.ExternalId, "Mã giao dịch ngân hàng");
        var shopCode = FinanceRules.Scope(input.ShopCode);
        actor.RequireShop(shopCode);
        var existing = await repository.FindBankTransactionByExternalIdAsync(externalId, shopCode, cancellationToken);
        if (existing is not null && (existing.Amount != input.Amount || existing.Currency != FinanceRules.Scope(input.Currency) || existing.CreditedOn != input.CreditedOn)) throw new InvalidOperationException("Bank transaction cùng mã có dữ liệu khác.");
        if (existing is null)
            await repository.AddBankTransactionAsync(new BankTransaction { ExternalId = externalId, ShopCode = shopCode, Currency = Required(input.Currency, "Currency").ToUpperInvariant(), Amount = NonZero(input.Amount), CreditedOn = input.CreditedOn }, cancellationToken);
    }

    public async Task<ReconciliationResult> CreateCaseAsync(CreateReconciliationCommand command, CancellationToken cancellationToken)
    {
        if (command.Tolerance < 0) throw new ArgumentException("Tolerance không được âm.");
        var payout = await repository.FindPayoutAsync(command.PayoutId, cancellationToken) ?? throw new ArgumentException("Không tìm thấy payout.");
        var bankTransaction = await repository.FindBankTransactionAsync(command.BankTransactionId, cancellationToken) ?? throw new ArgumentException("Không tìm thấy giao dịch ngân hàng.");
        if (!string.Equals(payout.ShopCode, bankTransaction.ShopCode, StringComparison.OrdinalIgnoreCase) || !string.Equals(payout.Currency, bankTransaction.Currency, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Payout và giao dịch ngân hàng phải cùng shop và currency.");
        if (string.IsNullOrWhiteSpace(command.Evidence)) throw new ArgumentException("Evidence là bắt buộc khi đề xuất đối soát.");

        var tolerance = await repository.GetToleranceAsync(payout.Currency, payout.PayoutOn, cancellationToken);
        if (command.Tolerance != tolerance) throw new ArgumentException("Tolerance phải theo policy server đang hiệu lực.");
        var banks = command.Banks?.ToList() ?? [new BankAllocationInput(bankTransaction.Id, bankTransaction.Amount)];
        foreach (var allocation in banks)
        {
            var source = await repository.FindBankTransactionAsync(allocation.BankTransactionId, cancellationToken) ?? throw new ArgumentException("Không tìm thấy bank allocation.");
            if (source.ShopCode != payout.ShopCode || source.Currency != payout.Currency || allocation.Amount <= 0) throw new ArgumentException("Bank allocation không cùng scope hoặc số tiền không hợp lệ.");
        }
        var difference = payout.Amount - banks.Sum(a => a.Amount);
        var reconciliationCase = new ReconciliationCase
        {
            PayoutId = payout.Id,
            BankTransactionId = bankTransaction.Id,
            Difference = difference,
            Tolerance = command.Tolerance,
            Evidence = command.Evidence.Trim(),
            ProposedBy = actor.UserName,
            Reason = Required(command.Reason ?? "", "Lý do ghép"),
            BankAllocations = banks.Select(a => new BankAllocation { BankTransactionId = a.BankTransactionId, Amount = a.Amount }).ToList(),
            StatementAllocations = (command.Allocations ?? []).Select(a => new StatementAllocation { ImportLineId = a.ImportLineId, Amount = a.Amount, OrderLineId = a.OrderLineId }).ToList(),
            Status = Math.Abs(difference) <= tolerance ? ReconciliationStatus.Candidate : ReconciliationStatus.Disputed,
        };
        await repository.AddCaseAsync(reconciliationCase, cancellationToken);
        return ToResult(reconciliationCase);
    }

    public async Task<ReconciliationResult> ConfirmAsync(Guid id, string reviewer, CancellationToken cancellationToken)
    {
        var reconciliationCase = await repository.FindCaseAsync(id, cancellationToken) ?? throw new ArgumentException("Không tìm thấy case đối soát.");
        actor.RequireRole("Checker");
        var payout = await repository.FindPayoutAsync(reconciliationCase.PayoutId, cancellationToken) ?? throw new ArgumentException("Không tìm thấy payout.");
        actor.RequireApproval(payout.Amount, "Reconciliation");
        var reviewedBy = actor.UserName;
        if (reconciliationCase.Status != ReconciliationStatus.Candidate) throw new ArgumentException("Chỉ case Candidate mới được xác nhận.");
        if (string.Equals(reconciliationCase.ProposedBy, reviewedBy, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Người đề xuất không được tự xác nhận.");
        if (reconciliationCase.StatementAllocations.Count == 0) throw new ArgumentException("Thiếu phân bổ statement. Chưa đủ chứng từ để FullyMatched.");
        if (reconciliationCase.StatementAllocations.Any(a => a.Amount > 0 && a.OrderLineId is null)) throw new ArgumentException("Dòng doanh thu chưa liên kết order line. Chưa đủ chứng từ để FullyMatched.");
        reconciliationCase.Status = ReconciliationStatus.FullyMatched;
        reconciliationCase.ReviewedBy = reviewedBy;
        await repository.SaveAsync(cancellationToken);
        return ToResult(reconciliationCase);
    }

    private static ReconciliationResult ToResult(ReconciliationCase value) => new(value.Id, value.Status.ToString(), value.Difference, value.Tolerance, value.ReviewedBy);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} là bắt buộc.") : value.Trim();
    private static decimal NonZero(decimal value) => value <= 0 ? throw new ArgumentException("Số tiền nhận phải lớn hơn 0.") : value;
}
