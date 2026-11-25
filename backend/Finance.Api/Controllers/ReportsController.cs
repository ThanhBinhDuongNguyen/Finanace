using System.Globalization;
using System.Text;
using Finance.Application.Security;
using Finance.Domain.Imports;
using Finance.Domain.Classification;
using Finance.Domain.Reconciliation;
using Finance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.Api.Controllers;

[ApiController, Route("api/finance/reports")]
public sealed class ReportsController(FinanceDbContext db, ICurrentActor actor) : ControllerBase
{
    [HttpGet("ledger")]
    public async Task<IActionResult> Ledger([FromQuery] string shopCode, [FromQuery] string currency, [FromQuery] DateOnly period, CancellationToken ct)
    {
        actor.RequireShop(shopCode);
        var shop = FinanceRules.Scope(shopCode); currency = FinanceRules.Scope(currency); period = FinanceRules.Month(period);
        var end = period.AddMonths(1);
        var entries = await db.JournalEntries.Include(j => j.Lines).Where(j => j.ShopCode == shop && j.Currency == currency && j.Period >= period && j.Period < end).ToListAsync(ct);
        var accounts = entries.SelectMany(j => j.Lines).GroupBy(l => l.AccountCode).Select(g => new { accountCode = g.Key, debit = g.Sum(l => l.Debit), credit = g.Sum(l => l.Credit) }).OrderBy(a => a.accountCode).ToList();
        var valid = await db.ImportBatches.Include(b => b.Lines).Where(b => b.ShopCode == shop && b.Currency == currency && b.Period >= period && b.Period < end).ToListAsync(ct);
        var classified = await db.ClassifiedTransactions.Where(t => t.Status == ClassificationStatus.Classified).Select(t => t.ImportLineId).ToListAsync(ct);
        var unknownCount = valid.SelectMany(b => b.Lines).Count(l => l.Status != ImportLineStatus.Valid || !classified.Contains(l.Id));
        decimal Net(string prefix) => accounts.Where(a => a.accountCode.StartsWith(prefix, StringComparison.Ordinal)).Sum(a => a.credit - a.debit);
        var bankIds = await db.BankTransactions.Where(b => b.ShopCode == shop && b.Currency == currency && b.CreditedOn >= period && b.CreditedOn < end).Select(b => b.Id).ToListAsync(ct);
        var matched = await db.ReconciliationCases.Where(c => c.Status == ReconciliationStatus.FullyMatched).Select(c => c.Id).ToListAsync(ct);
        var bankAllocations = await db.Set<BankAllocation>().Where(a => matched.Contains(a.ReconciliationCaseId) && bankIds.Contains(a.BankTransactionId)).ToListAsync(ct);
        return Ok(new { shopCode = shop, currency, period, snapshotAt = DateTimeOffset.UtcNow, definitionVersion = "ledger-v1", timezone = "UTC", unknownCount,
            sourceStatus = entries.Count == 0 ? "Missing" : unknownCount > 0 ? "Provisional" : "PostedLedger",
            grossRevenue = Net("511"), deductions = -Net("521"), netRevenue = Net("511") + Net("521"), cogs = -Net("632"), operatingExpense = -Net("641") - Net("642"),
            managementProfit = Net("511") + Net("521") + Net("632") + Net("641") + Net("642"),
            receivedCash = bankAllocations.Sum(a => a.Amount), accounts, entries });
    }
    [HttpGet("journals.csv")]
    public async Task<IActionResult> Export([FromQuery] string shopCode, CancellationToken ct)
    {
        actor.RequireShop(shopCode); var shop = FinanceRules.Scope(shopCode);
        var journals = await db.JournalEntries.Include(j => j.Lines).Where(j => j.ShopCode == shop).ToListAsync(ct);
        static string Cell(string value) => "\"" + ((value.TrimStart().StartsWith('=') || value.TrimStart().StartsWith('+') || value.TrimStart().StartsWith('-') || value.TrimStart().StartsWith('@')) ? "'" : "") + value.Replace("\"", "\"\"") + "\"";
        var csv = new StringBuilder("JournalId,Source,Period,Currency,Account,Debit,Credit\r\n");
        foreach (var journal in journals) foreach (var line in journal.Lines) csv.AppendLine(string.Join(",", Cell(journal.Id.ToString()), Cell(journal.ReferenceId), Cell(journal.Period.ToString("yyyy-MM-dd")), Cell(journal.Currency), Cell(line.AccountCode), line.Debit.ToString(CultureInfo.InvariantCulture), line.Credit.ToString(CultureInfo.InvariantCulture)));
        return File(Encoding.UTF8.GetBytes(csv.ToString()), "text/csv; charset=utf-8", "journals.csv");
    }
}
