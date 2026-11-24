using Finance.Domain.Reconciliation;

namespace Finance.Application.Reconciliation;

public sealed class ReconciliationService(IReconciliationRepository repository)
{
    public async Task AddPayoutAsync(PayoutInput input, CancellationToken cancellationToken)
    {
        var externalId = Required(input.ExternalId, "Mã payout");
        var shopCode = Required(input.ShopCode, "Shop");
        if (await repository.FindPayoutByExternalIdAsync(externalId, shopCode, cancellationToken) is null)
            await repository.AddPayoutAsync(new Payout { ExternalId = externalId, ShopCode = shopCode, Currency = Required(input.Currency, "Currency").ToUpperInvariant(), Amount = NonZero(input.Amount), PayoutOn = input.PayoutOn }, cancellationToken);
    }

    public async Task AddBankTransactionAsync(BankTransactionInput input, CancellationToken cancellationToken)
    {
        var externalId = Required(input.ExternalId, "Mã giao dịch ngân hàng");
        var shopCode = Required(input.ShopCode, "Shop");
        if (await repository.FindBankTransactionByExternalIdAsync(externalId, shopCode, cancellationToken) is null)
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

        var difference = payout.Amount - bankTransaction.Amount;
        var reconciliationCase = new ReconciliationCase
        {
            PayoutId = payout.Id,
            BankTransactionId = bankTransaction.Id,
            Difference = difference,
            Tolerance = command.Tolerance,
            Evidence = command.Evidence.Trim(),
            ProposedBy = Required(command.ProposedBy, "Người đề xuất"),
            Status = Math.Abs(difference) <= command.Tolerance ? ReconciliationStatus.Candidate : ReconciliationStatus.Disputed,
        };
        await repository.AddCaseAsync(reconciliationCase, cancellationToken);
        return ToResult(reconciliationCase);
    }

    public async Task<ReconciliationResult> ConfirmAsync(Guid id, string reviewer, CancellationToken cancellationToken)
    {
        var reconciliationCase = await repository.FindCaseAsync(id, cancellationToken) ?? throw new ArgumentException("Không tìm thấy case đối soát.");
        var reviewedBy = Required(reviewer, "Người duyệt");
        if (reconciliationCase.Status != ReconciliationStatus.Candidate) throw new ArgumentException("Chỉ case Candidate mới được xác nhận.");
        if (string.Equals(reconciliationCase.ProposedBy, reviewedBy, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Người đề xuất không được tự xác nhận.");
        reconciliationCase.Status = ReconciliationStatus.FullyMatched;
        reconciliationCase.ReviewedBy = reviewedBy;
        await repository.SaveAsync(cancellationToken);
        return ToResult(reconciliationCase);
    }

    private static ReconciliationResult ToResult(ReconciliationCase value) => new(value.Id, value.Status.ToString(), value.Difference, value.Tolerance, value.ReviewedBy);
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} là bắt buộc.") : value.Trim();
    private static decimal NonZero(decimal value) => value == 0 ? throw new ArgumentException("Số tiền phải khác 0.") : value;
}
