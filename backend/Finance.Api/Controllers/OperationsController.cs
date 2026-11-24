using Finance.Application.Exceptions;
using Finance.Application.Imports;
using Finance.Application.Ledger;
using Finance.Application.Reconciliation;
using Finance.Domain.Exceptions;
using Finance.Domain.Reconciliation;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/operations")]
public sealed class OperationsController(
    IImportBatchRepository importBatches,
    IReconciliationRepository reconciliations,
    IFinanceExceptionRepository exceptions,
    ILedgerRepository ledger) : ControllerBase
{
    [HttpGet("summary")]
    public async Task<ActionResult> Summary(CancellationToken cancellationToken)
    {
        var batches = await importBatches.GetAllAsync(cancellationToken);
        var cases = await reconciliations.GetCasesAsync(cancellationToken);
        var exceptionCases = await exceptions.GetAllAsync(cancellationToken);
        var journals = await ledger.GetJournalsAsync(cancellationToken);
        return Ok(new
        {
            importBatchCount = batches.Count,
            disputedCaseCount = cases.Count(value => value.Status == ReconciliationStatus.Disputed),
            openExceptionCount = exceptionCases.Count(value => value.Status != FinanceExceptionStatus.Closed),
            postedJournalCount = journals.Count,
        });
    }
}
