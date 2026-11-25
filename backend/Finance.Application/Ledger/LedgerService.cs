using Finance.Domain.Ledger;
using Finance.Application.Security;

namespace Finance.Application.Ledger;

public sealed class LedgerService(ILedgerRepository repository, ICurrentActor actor)
{
    public async Task<JournalResult> PostAsync(CreateJournalCommand command, CancellationToken cancellationToken)
    {
        var shopCode = FinanceRules.Scope(command.ShopCode);
        actor.RequireShop(shopCode);
        command = command with { Period = FinanceRules.Month(command.Period), PostedBy = actor.UserName };
        Required(command.Reason, "Lý do bút toán"); Required(command.Evidence, "Chứng từ bút toán");
        var existing = await repository.FindSourceAsync(shopCode, command.ReferenceType.Trim(), command.ReferenceId.Trim(), cancellationToken);
        if (existing is not null)
        {
            var sameLines = existing.Lines.Select(l => (l.AccountCode, l.Debit, l.Credit)).OrderBy(l => l.AccountCode).SequenceEqual(command.Lines.Select(l => (l.AccountCode, l.Debit, l.Credit)).OrderBy(l => l.AccountCode));
            if (!sameLines || existing.Currency != command.Currency.Trim().ToUpperInvariant() || existing.Period != command.Period) throw new InvalidOperationException("Nguồn journal đã tồn tại với nội dung khác.");
            return ToResult(existing);
        }
        if (await repository.IsPeriodClosedAsync(shopCode, command.Period, cancellationToken)) throw new ArgumentException("Kỳ đã khóa, không thể post journal mới.");
        if (command.Lines.Count < 2) throw new ArgumentException("Journal cần ít nhất hai dòng.");
        var lines = command.Lines.Select(line => new JournalLine { JournalEntryId = Guid.Empty, AccountCode = Required(line.AccountCode, "Account code"), Debit = PositiveOrZero(line.Debit), Credit = PositiveOrZero(line.Credit) }).ToList();
        if (lines.Any(line => (line.Debit == 0) == (line.Credit == 0))) throw new ArgumentException("Mỗi dòng chỉ có Nợ hoặc Có và phải lớn hơn 0.");
        if (lines.Sum(line => line.Debit) != lines.Sum(line => line.Credit)) throw new ArgumentException("Tổng Nợ phải bằng tổng Có.");

        var entryId = Guid.NewGuid();
        var entry = new JournalEntry
        {
            Id = entryId,
            Reason = command.Reason.Trim(), Evidence = command.Evidence.Trim(),
            ReferenceType = Required(command.ReferenceType, "Loại tham chiếu"),
            ReferenceId = Required(command.ReferenceId, "Mã tham chiếu"),
            ShopCode = shopCode,
            Currency = Required(command.Currency, "Currency").ToUpperInvariant(),
            Period = command.Period,
            PostedBy = Required(command.PostedBy, "Người post"),
            Lines = lines.Select(line => new JournalLine { JournalEntryId = entryId, AccountCode = line.AccountCode, Debit = line.Debit, Credit = line.Credit }).ToList(),
        };
        await repository.AddJournalAsync(entry, cancellationToken);
        return ToResult(entry);
    }

    public async Task<JournalResult> ReverseAsync(Guid id, ReverseJournalCommand command, CancellationToken cancellationToken)
    {
        command = command with { Period = FinanceRules.Month(command.Period), PostedBy = actor.UserName };
        var original = await repository.FindJournalAsync(id, cancellationToken) ?? throw new ArgumentException("Không tìm thấy journal gốc.");
        if (original.Status != JournalStatus.Posted) throw new ArgumentException("Journal đã bị đảo hoặc không thể đảo.");
        if (await repository.IsPeriodClosedAsync(original.ShopCode, command.Period, cancellationToken)) throw new ArgumentException("Kỳ điều chỉnh đã khóa.");
        var entryId = Guid.NewGuid();
        var reversal = new JournalEntry
        {
            Id = entryId,
            ReversesJournalEntryId = original.Id,
            Reason = "Reversal: " + original.Reason, Evidence = original.Evidence,
            ReferenceType = "JournalReversal",
            ReferenceId = original.Id.ToString(),
            ShopCode = original.ShopCode,
            Currency = original.Currency,
            Period = command.Period,
            PostedBy = Required(command.PostedBy, "Người post"),
            Lines = original.Lines.Select(line => new JournalLine { JournalEntryId = entryId, AccountCode = line.AccountCode, Debit = line.Credit, Credit = line.Debit }).ToList(),
        };
        original.Status = JournalStatus.Reversed;
        await repository.AddJournalAsync(reversal, cancellationToken);
        await repository.SaveAsync(cancellationToken);
        return ToResult(reversal);
    }

    public async Task ClosePeriodAsync(ClosePeriodCommand command, CancellationToken cancellationToken)
    {
        actor.RequireRole("Controller");
        var shopCode = FinanceRules.Scope(command.ShopCode);
        actor.RequireShop(shopCode);
        command = command with { Period = FinanceRules.Month(command.Period), ClosedBy = actor.UserName };
        if (!await repository.IsPeriodClosedAsync(shopCode, command.Period, cancellationToken))
            await repository.AddClosedPeriodAsync(new ClosedPeriod { ShopCode = shopCode, Period = command.Period, ClosedBy = Required(command.ClosedBy, "Người khóa kỳ") }, cancellationToken);
    }

    private static JournalResult ToResult(JournalEntry entry) => new(entry.Id, entry.Status.ToString(), entry.Lines.Sum(line => line.Debit), entry.Lines.Sum(line => line.Credit), entry.ReversesJournalEntryId);
    private static decimal PositiveOrZero(decimal value) => value < 0 ? throw new ArgumentException("Số tiền không được âm.") : value;
    private static string Required(string value, string name) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{name} là bắt buộc.") : value.Trim();
}
