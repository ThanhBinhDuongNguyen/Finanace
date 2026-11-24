using Finance.Application.Classification;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/classification")]
public sealed class ClassificationController(ClassificationService service, IClassificationRepository repository) : ControllerBase
{
    [HttpPost("mappings")]
    public async Task<ActionResult> AddMapping(CreateMappingCommand command, CancellationToken cancellationToken) => await Execute(() => service.AddMappingAsync(command, cancellationToken));
    [HttpGet("mappings")]
    public async Task<ActionResult> GetMappings(CancellationToken cancellationToken) => Ok(await repository.GetMappingsAsync(cancellationToken));
    [HttpPost("batches/{id:guid}/classify")]
    public async Task<ActionResult<ClassificationResult>> Classify(Guid id, ClassifyBatchCommand command, CancellationToken cancellationToken)
    {
        try { return Ok(await service.ClassifyBatchAsync(id, command, cancellationToken)); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
    [HttpGet("transactions")]
    public async Task<ActionResult> GetTransactions(CancellationToken cancellationToken) => Ok(await repository.GetTransactionsAsync(cancellationToken));

    private async Task<ActionResult> Execute(Func<Task> action)
    {
        try { await action(); return NoContent(); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }
}
