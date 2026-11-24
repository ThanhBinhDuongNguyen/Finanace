using Finance.Application.Imports;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/import-batches")]
public sealed class ImportBatchesController(ImportBatchService service, IImportBatchRepository repository) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ImportBatchResult>> Create(CreateImportBatchCommand command, CancellationToken cancellationToken)
    {
        try { return Ok(await service.CreateAsync(command, cancellationToken)); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken cancellationToken) => Ok(await repository.GetAllAsync(cancellationToken));
}
