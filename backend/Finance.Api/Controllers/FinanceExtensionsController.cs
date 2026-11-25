using Finance.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

[ApiController]
[Route("api/finance/extensions")]
public sealed class FinanceExtensionsController(FinanceExtensionService service) : ControllerBase
{
    [HttpGet("orders")]
    public async Task<ActionResult> Orders([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.OrdersAsync(shopCode, ct));

    [HttpPost("orders")]
    public async Task<ActionResult> AddOrder(OrderLineCommand command, CancellationToken ct) => Ok(await service.AddOrderAsync(command, ct));

    [HttpGet("refunds")]
    public async Task<ActionResult> Refunds([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.RefundsAsync(shopCode, ct));

    [HttpPost("refunds/external-confirmations")]
    public async Task<ActionResult> ConfirmRefund(RefundCommand command, CancellationToken ct) => Ok(await service.ConfirmExternalRefundAsync(command, ct));

    [HttpGet("refunds/requests")]
    public async Task<ActionResult> RefundRequests([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.RefundRequestsAsync(shopCode, ct));

    [HttpPost("refunds/requests")]
    public async Task<ActionResult> RequestRefund(RefundRequestCommand command, CancellationToken ct) => Ok(await service.RequestRefundAsync(command, ct));

    [HttpPost("refunds/requests/{id:guid}/approve")]
    public async Task<ActionResult> ApproveRefund(Guid id, ApproveRefundRequestCommand command, CancellationToken ct) => Ok(await service.ApproveRefundAsync(id, command, ct));

    [HttpPost("returns/cost-adjustments")]
    public async Task<ActionResult> ReturnCost(ReturnCostCommand command, CancellationToken ct) => Ok(await service.RecordReturnCostAsync(command, ct));

    [HttpPost("failed-delivery-fees")]
    public async Task<ActionResult> FailedDeliveryFee(FailedDeliveryFeeCommand command, CancellationToken ct) => Ok(await service.RecordFailedDeliveryFeeAsync(command, ct));

    [HttpGet("returns-and-fees")]
    public async Task<ActionResult> ReturnsAndFees([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.ReturnsAndFeesAsync(shopCode, ct));

    [HttpGet("costs")]
    public async Task<ActionResult> Costs([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.CostsAsync(shopCode, ct));

    [HttpPost("costs/policies")]
    public async Task<ActionResult> AddCostPolicy(CostPolicyCommand command, CancellationToken ct) => Ok(await service.AddCostPolicyAsync(command, ct));

    [HttpPost("costs/layers")]
    public async Task<ActionResult> AddCostLayer(CostLayerCommand command, CancellationToken ct) => Ok(await service.AddCostLayerAsync(command, ct));

    [HttpPost("costs/assessments")]
    public async Task<ActionResult> AssessCost(AssessCostCommand command, CancellationToken ct) => Ok(await service.AssessCostAsync(command, ct));

    [HttpGet("documents")]
    public async Task<ActionResult> Documents([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.DocumentsAsync(shopCode, ct));

    [HttpPost("documents")]
    [RequestSizeLimit(7_000_000)]
    public async Task<ActionResult> AddDocument(DocumentCommand command, CancellationToken ct) => Ok(await service.AddDocumentAsync(command, ct));

    [HttpPost("documents/revisions/{id:guid}/review")]
    public async Task<ActionResult> ReviewDocument(Guid id, ReviewDocumentCommand command, CancellationToken ct) => Ok(await service.ReviewDocumentAsync(id, command, ct));

    [HttpGet("documents/revisions/{id:guid}/content")]
    public async Task<ActionResult> Download(Guid id, CancellationToken ct)
    {
        var file = await service.DownloadAsync(id, ct);
        return File(file.Content, "application/octet-stream", file.FileName);
    }

    [HttpGet("audit")]
    public async Task<ActionResult> Audit([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.AuditAsync(shopCode, ct));

    [HttpGet("payout-accounts")]
    public async Task<ActionResult> PayoutAccounts([FromQuery] string shopCode, CancellationToken ct) => Ok(await service.PayoutAccountsAsync(shopCode, ct));

    [HttpPost("payout-accounts/changes")]
    public async Task<ActionResult> ProposePayoutAccount(ProposePayoutAccountCommand command, CancellationToken ct) => Ok(await service.ProposePayoutAccountAsync(command, ct));

    [HttpPost("payout-accounts/changes/{id:guid}/approve")]
    public async Task<ActionResult> ApprovePayoutAccount(Guid id, ApprovePayoutAccountCommand command, CancellationToken ct) => Ok(await service.ApprovePayoutAccountAsync(id, command, ct));
}
