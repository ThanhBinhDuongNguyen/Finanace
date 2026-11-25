namespace Finance.Domain.Extensions;

// Read-only snapshots from OMS/WMS. Finance never changes physical stock or ATP.
public sealed class FinanceOrderLine
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ExternalId { get; init; } = "";
    public string OrderId { get; init; } = "";
    public string ShopCode { get; init; } = "";
    public string LegalEntity { get; init; } = "";
    public string Channel { get; init; } = "";
    public string Currency { get; init; } = "";
    public string Sku { get; init; } = "";
    public string SkuGroup { get; init; } = "";
    public decimal Quantity { get; init; }
    public decimal NetAmount { get; init; }
    public DateOnly OccurredOn { get; init; }
    public string Evidence { get; init; } = "";
    public string ImportedBy { get; init; } = "";
}

public sealed class RefundConfirmation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OrderLineId { get; init; }
    public string ShopCode { get; init; } = "";
    public string SourceRefundId { get; init; } = "";
    public decimal Amount { get; init; }
    public string Currency { get; init; } = "";
    public DateOnly ConfirmedOn { get; init; }
    public string Evidence { get; init; } = "";
    public string ConfirmedBy { get; init; } = "";
    public Guid JournalEntryId { get; init; }
    public Guid? ApprovedRequestId { get; init; }
    public DateTimeOffset RecordedAt { get; init; } = DateTimeOffset.UtcNow;
}

public enum FinanceRefundRequestStatus { Requested, Approved, Confirmed }

public sealed class FinanceRefundRequest
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string SourceRequestId { get; init; } = "";
    public Guid OrderLineId { get; init; }
    public decimal Amount { get; init; }
    public string RequestedBy { get; init; } = "";
    public string Reason { get; init; } = "";
    public string Evidence { get; init; } = "";
    public FinanceRefundRequestStatus Status { get; set; } = FinanceRefundRequestStatus.Requested;
    public string? ApprovedBy { get; set; }
    public string? ApprovalEvidence { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
}

public enum ReturnCostDisposition { Restock, Scrap }

public sealed class FinanceReturnCostAdjustment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string SourceQcId { get; init; } = "";
    public Guid OrderLineId { get; init; }
    public Guid FinalAssessmentId { get; init; }
    public ReturnCostDisposition Disposition { get; init; }
    public decimal Quantity { get; init; }
    public decimal Amount { get; init; }
    public DateOnly PostedOn { get; init; }
    public string Evidence { get; init; } = "";
    public string RecordedBy { get; init; } = "";
    public Guid? JournalEntryId { get; init; }
}

public sealed class FinanceFailedDeliveryFee
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string SourceFeeId { get; init; } = "";
    public string PackageId { get; init; } = "";
    public string Currency { get; init; } = "";
    public decimal Amount { get; init; }
    public DateOnly OccurredOn { get; init; }
    public string Evidence { get; init; } = "";
    public string RecordedBy { get; init; } = "";
    public Guid? ImportLineId { get; init; }
    public Guid? JournalEntryId { get; init; }
}

public enum CostMethod { Fifo, WeightedAverage }

public sealed class FinanceCostPolicy
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string SkuGroup { get; init; } = "";
    public DateOnly Period { get; init; }
    public CostMethod Method { get; init; }
    public string ApprovedBy { get; init; } = "";
    public string Evidence { get; init; } = "";
}

public sealed class FinanceCostLayer
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string Sku { get; init; } = "";
    public string Currency { get; init; } = "";
    public string SourceReceiptId { get; init; } = "";
    public decimal Quantity { get; init; }
    public decimal UsedQuantity { get; set; }
    public decimal UnitCost { get; init; }
    public DateOnly ReceivedOn { get; init; }
    public string Evidence { get; init; } = "";
}

public enum FinanceCostStatus { Temporary, Final }

public sealed class FinanceCostAssessment
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid OrderLineId { get; init; }
    public string ShopCode { get; init; } = "";
    public FinanceCostStatus Status { get; init; }
    public decimal Amount { get; init; }
    public decimal Adjustment { get; init; }
    public Guid? ReplacesAssessmentId { get; init; }
    public Guid? JournalEntryId { get; init; }
    public Guid? PolicyId { get; init; }
    public string Evidence { get; init; } = "";
    public string AssessedBy { get; init; } = "";
    public DateOnly PostedOn { get; init; }
    public List<FinanceCostAllocation> Allocations { get; init; } = [];
}

public sealed class FinanceCostAllocation
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid FinanceCostAssessmentId { get; init; }
    public Guid CostLayerId { get; init; }
    public decimal Quantity { get; init; }
    public decimal Amount { get; init; }
}

public enum FinanceDocumentStatus { PendingReview, Verified, Rejected }

public sealed class FinanceDocument
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string ExternalId { get; init; } = "";
    public string Kind { get; init; } = "";
    public Guid SourceId { get; init; }
    public List<FinanceDocumentRevision> Revisions { get; init; } = [];
}

public sealed class FinanceDocumentRevision
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public Guid FinanceDocumentId { get; init; }
    public int Version { get; init; }
    public string FileName { get; init; } = "";
    public byte[] Content { get; init; } = [];
    public string Sha256 { get; init; } = "";
    public string Reason { get; init; } = "";
    public string UploadedBy { get; init; } = "";
    public DateTimeOffset UploadedAt { get; init; } = DateTimeOffset.UtcNow;
    public FinanceDocumentStatus Status { get; set; } = FinanceDocumentStatus.PendingReview;
    public string? ReviewedBy { get; set; }
    public string? ReviewNote { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
}

public sealed class FinanceExtensionAudit
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public Guid SourceId { get; init; }
    public string Action { get; init; } = "";
    public string Actor { get; init; } = "";
    public string Evidence { get; init; } = "";
    public DateTimeOffset OccurredAt { get; init; } = DateTimeOffset.UtcNow;
}

public enum PayoutAccountChangeStatus { PendingApproval, Approved }

public sealed class PayoutAccountChange
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string SourceRequestId { get; init; } = "";
    public Guid? PreviousAccountId { get; init; }
    public string NewAccountReference { get; init; } = "";
    public string IndependentChannelEvidence { get; init; } = "";
    public string Reason { get; init; } = "";
    public string ProposedBy { get; init; } = "";
    public DateTimeOffset ProposedAt { get; init; } = DateTimeOffset.UtcNow;
    public PayoutAccountChangeStatus Status { get; set; } = PayoutAccountChangeStatus.PendingApproval;
    public string? ApprovedBy { get; set; }
    public DateTimeOffset? ApprovedAt { get; set; }
    public string? ApprovalEvidence { get; set; }
}

public sealed class PayoutReceivingAccount
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string ShopCode { get; init; } = "";
    public string AccountReference { get; init; } = "";
    public Guid ApprovedChangeId { get; init; }
    public DateTimeOffset ActivatedAt { get; init; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? DeactivatedAt { get; set; }
}
