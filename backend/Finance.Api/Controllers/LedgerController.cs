using Finance.Application.Ledger;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/ledger")]
public sealed class LedgerController(LedgerService service, ILedgerRepository repository) : ControllerBase
{
    [HttpPost("journals")]
    public async Task<ActionResult<JournalResult>> Post(CreateJournalCommand command, CancellationToken cancellationToken) => await Execute(() => service.PostAsync(command, cancellationToken));
    [HttpPost("journals/{id:guid}/reverse")]
    public async Task<ActionResult<JournalResult>> Reverse(Guid id, ReverseJournalCommand command, CancellationToken cancellationToken) => await Execute(() => service.ReverseAsync(id, command, cancellationToken));
    [HttpPost("periods/close")]
    public async Task<ActionResult> ClosePeriod(ClosePeriodCommand command, CancellationToken cancellationToken)
    {
        try { await service.ClosePeriodAsync(command, cancellationToken); return NoContent(); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
    [HttpGet("journals")]
    public async Task<ActionResult> GetJournals(CancellationToken cancellationToken) => Ok(await repository.GetJournalsAsync(cancellationToken));

    private async Task<ActionResult<JournalResult>> Execute(Func<Task<JournalResult>> action)
    {
        try { return Ok(await action()); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
