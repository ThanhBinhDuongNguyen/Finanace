using Finance.Application.Imports;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/import-batches")]
public sealed class ImportBatchesController(ImportBatchService service, IImportBatchRepository repository) : ControllerBase
{
    [HttpPost("preview")]
    public async Task<IActionResult> Preview(CreateImportBatchCommand command, CancellationToken cancellationToken)
    {
        var batch = await service.PreviewAsync(command, cancellationToken);
        return Ok(new { batch.Checksum, batch.Lines, validLines = batch.Lines.Count(l => l.Status == Finance.Domain.Imports.ImportLineStatus.Valid), quarantinedLines = batch.Lines.Count(l => l.Status == Finance.Domain.Imports.ImportLineStatus.Quarantined), isDuplicate = await repository.FindByChecksumAsync(batch.Checksum, cancellationToken) is not null });
    }
    [HttpPost]
    public async Task<ActionResult<ImportBatchResult>> Create(CreateImportBatchCommand command, CancellationToken cancellationToken)
    {
        try { return Ok(await service.CreateAsync(command, cancellationToken)); }
        catch (ArgumentException exception) { return BadRequest(new { message = exception.Message }); }
    }

    [HttpGet]
    public async Task<ActionResult> GetAll(CancellationToken cancellationToken) => Ok(await repository.GetAllAsync(cancellationToken));
}
