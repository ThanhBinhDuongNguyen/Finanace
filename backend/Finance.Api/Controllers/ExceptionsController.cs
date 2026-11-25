using Finance.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/exceptions")]
public sealed class ExceptionsController(ExceptionService service, IFinanceExceptionRepository repository) : ControllerBase
{
    [HttpPost("{id:guid}/wait-external")]
    public async Task<ActionResult<ExceptionResult>> WaitExternal(Guid id, ExceptionActionCommand command, CancellationToken ct) => await Execute(() => service.WaitExternalAsync(id, command, ct));
    [HttpPost]
    public async Task<ActionResult<ExceptionResult>> Create(CreateExceptionCommand command, CancellationToken cancellationToken) => await Execute(() => service.CreateAsync(command, cancellationToken));
    [HttpPost("{id:guid}/request-closure")]
    public async Task<ActionResult<ExceptionResult>> RequestClosure(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken) => await Execute(() => service.RequestClosureAsync(id, command, cancellationToken));
    [HttpPost("{id:guid}/approve-closure")]
    public async Task<ActionResult<ExceptionResult>> ApproveClosure(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken) => await Execute(() => service.ApproveClosureAsync(id, command, cancellationToken));
    [HttpPost("{id:guid}/reopen")]
    public async Task<ActionResult<ExceptionResult>> Reopen(Guid id, ExceptionActionCommand command, CancellationToken cancellationToken) => await Execute(() => service.ReopenAsync(id, command, cancellationToken));
    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken cancellationToken) => Ok(await repository.GetAllAsync(cancellationToken));

    private async Task<ActionResult<ExceptionResult>> Execute(Func<Task<ExceptionResult>> action)
    {
        try { return Ok(await action()); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
