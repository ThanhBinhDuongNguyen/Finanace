using Finance.Application.Reconciliation;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/reconciliation")]
public sealed class ReconciliationController(ReconciliationService service, IReconciliationRepository repository) : ControllerBase
{
    [HttpPost("payouts")]
    public async Task<ActionResult> AddPayout(PayoutInput input, CancellationToken cancellationToken) => await Execute(() => service.AddPayoutAsync(input, cancellationToken));

    [HttpGet("payouts")]
    public async Task<ActionResult> GetPayouts(CancellationToken cancellationToken) => Ok(await repository.GetPayoutsAsync(cancellationToken));

    [HttpPost("bank-transactions")]
    public async Task<ActionResult> AddBankTransaction(BankTransactionInput input, CancellationToken cancellationToken) => await Execute(() => service.AddBankTransactionAsync(input, cancellationToken));

    [HttpGet("bank-transactions")]
    public async Task<ActionResult> GetBankTransactions(CancellationToken cancellationToken) => Ok(await repository.GetBankTransactionsAsync(cancellationToken));

    [HttpPost("cases")]
    public async Task<ActionResult<ReconciliationResult>> CreateCase(CreateReconciliationCommand command, CancellationToken cancellationToken)
    {
        try { return Ok(await service.CreateCaseAsync(command, cancellationToken)); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [HttpPost("cases/{id:guid}/confirm")]
    public async Task<ActionResult<ReconciliationResult>> Confirm(Guid id, [FromBody] string reviewer, CancellationToken cancellationToken)
    {
        try { return Ok(await service.ConfirmAsync(id, reviewer, cancellationToken)); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [HttpGet("cases")]
    public async Task<ActionResult> GetCases(CancellationToken cancellationToken) => Ok(await repository.GetCasesAsync(cancellationToken));

    private async Task<ActionResult> Execute(Func<Task> action)
    {
        try { await action(); return NoContent(); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
