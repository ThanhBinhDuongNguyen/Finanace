using System.Security.Cryptography;
using Finance.Application.Security;
using Finance.Domain.Extensions;
using Finance.Domain.Imports;
using Finance.Domain.Ledger;
using Finance.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Finance.Infrastructure.Extensions;

public sealed record OrderLineCommand(string ExternalId, string OrderId, string ShopCode, string LegalEntity, string Channel, string Currency, string Sku, string SkuGroup, decimal Quantity, decimal NetAmount, DateOnly OccurredOn, string Evidence);
public sealed record RefundCommand(Guid OrderLineId, string SourceRefundId, decimal Amount, DateOnly ConfirmedOn, string Evidence, Guid? ApprovedRequestId = null);
public sealed record RefundRequestCommand(Guid OrderLineId, string SourceRequestId, decimal Amount, string Reason, string Evidence);
public sealed record ApproveRefundRequestCommand(string Evidence);
public sealed record ReturnCostCommand(Guid OrderLineId, string SourceQcId, ReturnCostDisposition Disposition, decimal Quantity, DateOnly PostedOn, string Evidence);
public sealed record FailedDeliveryFeeCommand(string ShopCode, string SourceFeeId, string PackageId, string Currency, decimal Amount, DateOnly OccurredOn, string Evidence);
public sealed record CostPolicyCommand(string ShopCode, string SkuGroup, DateOnly Period, CostMethod Method, string Evidence);
public sealed record CostLayerCommand(string ShopCode, string Sku, string Currency, string SourceReceiptId, decimal Quantity, decimal UnitCost, DateOnly ReceivedOn, string Evidence);
public sealed record AssessCostCommand(Guid OrderLineId, bool IsTemporary, decimal? EstimatedAmount, DateOnly PostedOn, string Evidence);
public sealed record DocumentCommand(string ShopCode, string ExternalId, string Kind, Guid SourceId, string FileName, string ContentBase64, string Reason);
public sealed record ReviewDocumentCommand(bool Approve, string Note);
public sealed record ProposePayoutAccountCommand(string ShopCode, string SourceRequestId, string NewAccountReference, string IndependentChannelEvidence, string Reason);
public sealed record ApprovePayoutAccountCommand(string Evidence);

public sealed class FinanceExtensionService(FinanceDbContext db, ICurrentActor actor)
{
    public static void ConfigureScope(ModelBuilder model, FinanceDbContext context)
    {
        model.Entity<FinanceOrderLine>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<RefundConfirmation>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceRefundRequest>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceReturnCostAdjustment>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceFailedDeliveryFee>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceCostPolicy>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceCostLayer>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceCostAssessment>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceDocument>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<PayoutAccountChange>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<PayoutReceivingAccount>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceExtensionAudit>().HasQueryFilter(x => context.AllowedShops.Contains(x.ShopCode));
        model.Entity<FinanceCostAllocation>().HasQueryFilter(x => context.Set<FinanceCostAssessment>().Any(parent => parent.Id == x.FinanceCostAssessmentId));
        model.Entity<FinanceDocumentRevision>().HasQueryFilter(x => context.Set<FinanceDocument>().Any(parent => parent.Id == x.FinanceDocumentId));
    }

    public static void ConfigureModel(ModelBuilder model)
    {
        model.Entity<FinanceOrderLine>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.ExternalId }).IsUnique();
        });
        model.Entity<RefundConfirmation>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.SourceRefundId }).IsUnique();
            e.HasOne<FinanceOrderLine>().WithMany().HasForeignKey(x => x.OrderLineId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<JournalEntry>().WithMany().HasForeignKey(x => x.JournalEntryId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<FinanceRefundRequest>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.SourceRequestId }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().IsConcurrencyToken();
            e.HasOne<FinanceOrderLine>().WithMany().HasForeignKey(x => x.OrderLineId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<FinanceReturnCostAdjustment>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.SourceQcId }).IsUnique();
            e.Property(x => x.Disposition).HasConversion<string>();
            e.HasOne<FinanceOrderLine>().WithMany().HasForeignKey(x => x.OrderLineId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne<FinanceCostAssessment>().WithMany().HasForeignKey(x => x.FinalAssessmentId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<FinanceFailedDeliveryFee>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.SourceFeeId }).IsUnique();
        });
        model.Entity<FinanceCostPolicy>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.SkuGroup, x.Period }).IsUnique();
            e.Property(x => x.Method).HasConversion<string>();
        });
        model.Entity<FinanceCostLayer>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.Sku, x.SourceReceiptId }).IsUnique();
            e.Property(x => x.UsedQuantity).IsConcurrencyToken();
        });
        model.Entity<FinanceCostAssessment>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.Property(x => x.Status).HasConversion<string>();
            e.HasIndex(x => new { x.OrderLineId, x.Status }).IsUnique();
            e.HasOne<FinanceOrderLine>().WithMany().HasForeignKey(x => x.OrderLineId).OnDelete(DeleteBehavior.Restrict);
            e.HasMany(x => x.Allocations).WithOne().HasForeignKey(x => x.FinanceCostAssessmentId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<FinanceCostAllocation>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasOne<FinanceCostLayer>().WithMany().HasForeignKey(x => x.CostLayerId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<FinanceDocument>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.ExternalId }).IsUnique();
            e.HasMany(x => x.Revisions).WithOne().HasForeignKey(x => x.FinanceDocumentId).OnDelete(DeleteBehavior.Restrict);
        });
        model.Entity<FinanceDocumentRevision>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.FinanceDocumentId, x.Version }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().IsConcurrencyToken();
        });
        model.Entity<FinanceExtensionAudit>(e => { e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever(); });
        model.Entity<PayoutAccountChange>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => new { x.ShopCode, x.SourceRequestId }).IsUnique();
            e.Property(x => x.Status).HasConversion<string>().IsConcurrencyToken();
        });
        model.Entity<PayoutReceivingAccount>(e =>
        {
            e.HasKey(x => x.Id); e.Property(x => x.Id).ValueGeneratedNever();
            e.HasIndex(x => x.ShopCode).IsUnique().HasFilter("\"DeactivatedAt\" IS NULL");
            e.HasIndex(x => x.ApprovedChangeId).IsUnique();
            e.Property(x => x.DeactivatedAt).IsConcurrencyToken();
            e.HasOne<PayoutAccountChange>().WithMany().HasForeignKey(x => x.ApprovedChangeId).OnDelete(DeleteBehavior.Restrict);
        });
    }

    public async Task<FinanceOrderLine> AddOrderAsync(OrderLineCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin");
        var shop = Scope(input.ShopCode);
        var externalId = Required(input.ExternalId, "ExternalId");
        var order = new FinanceOrderLine
        {
            ExternalId = externalId, OrderId = Required(input.OrderId, "OrderId"), ShopCode = shop,
            LegalEntity = Required(input.LegalEntity, "LegalEntity"), Channel = Required(input.Channel, "Channel"),
            Currency = Currency(input.Currency), Sku = Required(input.Sku, "Sku"), SkuGroup = Required(input.SkuGroup, "SkuGroup"),
            Quantity = Positive(input.Quantity), NetAmount = Money(input.NetAmount), OccurredOn = ValidDate(input.OccurredOn),
            Evidence = Required(input.Evidence, "Evidence"), ImportedBy = actor.UserName,
        };
        var existing = await db.Set<FinanceOrderLine>().SingleOrDefaultAsync(x => x.ShopCode == shop && x.ExternalId == externalId, ct);
        if (existing is not null)
        {
            if (existing.OrderId != order.OrderId || existing.Currency != order.Currency || existing.Quantity != order.Quantity || existing.NetAmount != order.NetAmount || existing.Sku != order.Sku || existing.LegalEntity != order.LegalEntity || existing.OccurredOn != order.OccurredOn)
                throw new ArgumentException("Order line source ID already exists with different data. Submit a correction event instead.");
            return existing;
        }
        db.Add(order); Audit(shop, order.Id, "OrderSnapshotImported", order.Evidence);
        await db.SaveChangesAsync(ct); return order;
    }

    public async Task<IReadOnlyList<FinanceOrderLine>> OrdersAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        return await db.Set<FinanceOrderLine>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct);
    }

    public async Task<RefundConfirmation> ConfirmExternalRefundAsync(RefundCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin");
        // The marketplace has already refunded. This records the outcome; it never sends a payment.
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var order = await OrderAsync(input.OrderLineId, ct);
        var sourceId = Required(input.SourceRefundId, "SourceRefundId");
        var amount = Money(input.Amount); var date = ValidDate(input.ConfirmedOn);
        var evidence = Required(input.Evidence, "Evidence");
        var previous = await db.Set<RefundConfirmation>().SingleOrDefaultAsync(x => x.ShopCode == order.ShopCode && x.SourceRefundId == sourceId, ct);
        if (previous is not null)
        {
            if (previous.OrderLineId != order.Id || previous.Amount != amount || previous.ConfirmedOn != date)
                throw new ArgumentException("Refund source ID conflicts with the recorded confirmation.");
            return previous;
        }
        var recorded = await db.Set<RefundConfirmation>().Where(x => x.OrderLineId == order.Id).ToListAsync(ct);
        if (recorded.Sum(x => x.Amount) + amount > order.NetAmount)
            throw new ArgumentException("Refund exceeds the remaining eligible order amount.");
        FinanceRefundRequest? approvedRequest = null;
        if (input.ApprovedRequestId is not null)
        {
            approvedRequest = await db.Set<FinanceRefundRequest>().SingleOrDefaultAsync(x => x.Id == input.ApprovedRequestId && x.OrderLineId == order.Id, ct)
                ?? throw new ArgumentException("Approved refund request not found for this order line.");
            if (approvedRequest.Status != FinanceRefundRequestStatus.Approved || approvedRequest.Amount != amount)
                throw new ArgumentException("Confirmation must match an approved, unconfirmed refund request.");
            approvedRequest.Status = FinanceRefundRequestStatus.Confirmed;
        }
        else if (order.Channel.Equals("D2C", StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("D2C refund confirmation requires an approved internal request.");
        await EnsureOpenAsync(order.ShopCode, date, ct);
        var refundId = Guid.NewGuid();
        var journal = Journal(order.ShopCode, order.Currency, date, "ExternalRefund", refundId.ToString(), "521", "131", amount);
        var refund = new RefundConfirmation { Id = refundId, OrderLineId = order.Id, ShopCode = order.ShopCode, SourceRefundId = sourceId, Amount = amount, Currency = order.Currency, ConfirmedOn = date, Evidence = evidence, ConfirmedBy = actor.UserName, JournalEntryId = journal.Id, ApprovedRequestId = approvedRequest?.Id };
        db.Add(journal); db.Add(refund); Audit(order.ShopCode, refund.Id, "ExternalRefundConfirmed", evidence);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return refund;
    }

    public async Task<IReadOnlyList<RefundConfirmation>> RefundsAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        return await db.Set<RefundConfirmation>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct);
    }

    public async Task<FinanceRefundRequest> RequestRefundAsync(RefundRequestCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var order = await OrderAsync(input.OrderLineId, ct); var sourceId = Required(input.SourceRequestId, "SourceRequestId"); var amount = Money(input.Amount);
        var prior = await db.Set<FinanceRefundRequest>().SingleOrDefaultAsync(x => x.ShopCode == order.ShopCode && x.SourceRequestId == sourceId, ct);
        if (prior is not null)
        {
            if (prior.OrderLineId != order.Id || prior.Amount != amount) throw new ArgumentException("Refund request identity conflicts with existing data.");
            return prior;
        }
        var recorded = await db.Set<RefundConfirmation>().Where(x => x.OrderLineId == order.Id).ToListAsync(ct);
        var open = await db.Set<FinanceRefundRequest>().Where(x => x.OrderLineId == order.Id && x.Status != FinanceRefundRequestStatus.Confirmed).ToListAsync(ct);
        if (recorded.Sum(x => x.Amount) + open.Sum(x => x.Amount) + amount > order.NetAmount) throw new ArgumentException("Open refund requests exceed the remaining eligible amount.");
        var request = new FinanceRefundRequest { ShopCode = order.ShopCode, OrderLineId = order.Id, SourceRequestId = sourceId, Amount = amount, RequestedBy = actor.UserName, Reason = Required(input.Reason, "Reason"), Evidence = Required(input.Evidence, "Evidence") };
        db.Add(request); Audit(order.ShopCode, request.Id, "RefundRequested", request.Evidence); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return request;
    }

    public async Task<FinanceRefundRequest> ApproveRefundAsync(Guid id, ApproveRefundRequestCommand input, CancellationToken ct)
    {
        actor.RequireRole("Checker", "Controller", "Admin");
        var request = await db.Set<FinanceRefundRequest>().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new ArgumentException("Refund request not found."); Scope(request.ShopCode);
        actor.RequireApproval(request.Amount, "Refund");
        if (request.RequestedBy.Equals(actor.UserName, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Requester cannot approve their own refund.");
        if (request.Status != FinanceRefundRequestStatus.Requested) throw new ArgumentException("Refund request is not awaiting approval.");
        request.Status = FinanceRefundRequestStatus.Approved; request.ApprovedBy = actor.UserName; request.ApprovalEvidence = Required(input.Evidence, "Approval evidence"); request.ApprovedAt = DateTimeOffset.UtcNow;
        Audit(request.ShopCode, request.Id, "RefundApprovedForExternalExecution", request.ApprovalEvidence); await db.SaveChangesAsync(ct); return request;
    }

    public async Task<IReadOnlyList<FinanceRefundRequest>> RefundRequestsAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode); return await db.Set<FinanceRefundRequest>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct);
    }

    public async Task<FinanceReturnCostAdjustment> RecordReturnCostAsync(ReturnCostCommand input, CancellationToken ct)
    {
        actor.RequireRole("Controller", "Admin");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var order = await OrderAsync(input.OrderLineId, ct); var qcId = Required(input.SourceQcId, "SourceQcId"); var quantity = Positive(input.Quantity);
        var date = ValidDate(input.PostedOn); var evidence = Required(input.Evidence, "QC evidence");
        if (!Enum.IsDefined(input.Disposition)) throw new ArgumentException("Unsupported return disposition.");
        var existing = await db.Set<FinanceReturnCostAdjustment>().SingleOrDefaultAsync(x => x.ShopCode == order.ShopCode && x.SourceQcId == qcId, ct);
        if (existing is not null)
        {
            if (existing.OrderLineId != order.Id || existing.Quantity != quantity || existing.Disposition != input.Disposition || existing.PostedOn != date) throw new ArgumentException("QC event identity conflicts with recorded return.");
            return existing;
        }
        var final = await db.Set<FinanceCostAssessment>().SingleOrDefaultAsync(x => x.OrderLineId == order.Id && x.Status == FinanceCostStatus.Final, ct)
            ?? throw new ArgumentException("A final cost assessment is required before return cost adjustment.");
        var returns = await db.Set<FinanceReturnCostAdjustment>().Where(x => x.OrderLineId == order.Id).ToListAsync(ct);
        var totalQty = returns.Sum(x => x.Quantity);
        if (totalQty + quantity > order.Quantity) throw new ArgumentException("Cumulative returned quantity exceeds the sold quantity.");
        var amount = input.Disposition == ReturnCostDisposition.Scrap ? 0 : decimal.Round(final.Amount * quantity / order.Quantity, 2, MidpointRounding.AwayFromZero);
        var restocked = returns.Where(x => x.Disposition == ReturnCostDisposition.Restock).ToList();
        if (input.Disposition == ReturnCostDisposition.Restock && restocked.Sum(x => x.Quantity) + quantity == order.Quantity) amount = final.Amount - restocked.Sum(x => x.Amount);
        if (restocked.Sum(x => x.Amount) + amount > final.Amount) throw new ArgumentException("Cost reversal exceeds the final recognized cost.");
        await EnsureOpenAsync(order.ShopCode, date, ct);
        var id = Guid.NewGuid(); var journal = amount == 0 ? null : Journal(order.ShopCode, order.Currency, date, "ReturnCostQc", id.ToString(), "156", "632", amount);
        var adjustment = new FinanceReturnCostAdjustment { Id = id, ShopCode = order.ShopCode, SourceQcId = qcId, OrderLineId = order.Id, FinalAssessmentId = final.Id, Disposition = input.Disposition, Quantity = quantity, Amount = amount, PostedOn = date, Evidence = evidence, RecordedBy = actor.UserName, JournalEntryId = journal?.Id };
        if (journal is not null) db.Add(journal); db.Add(adjustment); Audit(order.ShopCode, id, "ReturnQcCostAdjusted", evidence);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return adjustment;
    }

    public async Task<FinanceFailedDeliveryFee> RecordFailedDeliveryFeeAsync(FailedDeliveryFeeCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var shop = Scope(input.ShopCode); var source = Required(input.SourceFeeId, "SourceFeeId"); var package = Required(input.PackageId, "PackageId");
        var currency = Currency(input.Currency); var amount = Money(input.Amount); var date = ValidDate(input.OccurredOn); var evidence = Required(input.Evidence, "Fee evidence");
        var existing = await db.Set<FinanceFailedDeliveryFee>().SingleOrDefaultAsync(x => x.ShopCode == shop && x.SourceFeeId == source, ct);
        if (existing is not null)
        {
            if (existing.PackageId != package || existing.Amount != amount || existing.Currency != currency || existing.OccurredOn != date) throw new ArgumentException("Source fee conflicts with the existing delivery fee.");
            return existing;
        }
        var statement = await db.ImportLines.Join(db.ImportBatches, line => line.ImportBatchId, batch => batch.Id, (line, batch) => new { line, batch }).Where(x => x.batch.ShopCode == shop && x.line.ExternalId == source && x.line.Status == ImportLineStatus.Valid).ToListAsync(ct);
        if (statement.Count > 1) throw new ArgumentException("Ambiguous source fee. Reconcile statement duplicates first.");
        var sourceLine = statement.SingleOrDefault();
        if (sourceLine is not null && (sourceLine.batch.Currency != currency || Math.Abs(sourceLine.line.Amount) != amount || !sourceLine.line.BusinessType.Contains("fee", StringComparison.OrdinalIgnoreCase))) throw new ArgumentException("Statement source ID exists with different currency, amount, or business type.");
        await EnsureOpenAsync(shop, date, ct);
        var id = Guid.NewGuid();
        var journal = sourceLine is null ? Journal(shop, currency, date, "FailedDeliveryFee", source, "641", "338", amount) : null;
        var fee = new FinanceFailedDeliveryFee { Id = id, ShopCode = shop, SourceFeeId = source, PackageId = package, Currency = currency, Amount = amount, OccurredOn = date, Evidence = evidence, RecordedBy = actor.UserName, ImportLineId = sourceLine?.line.Id, JournalEntryId = journal?.Id };
        if (journal is not null) db.Add(journal); db.Add(fee); Audit(shop, id, sourceLine is null ? "FailedDeliveryFeeRecorded" : "FailedDeliveryFeeLinkedToStatement", evidence);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return fee;
    }

    public async Task<object> ReturnsAndFeesAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        return new { returns = await db.Set<FinanceReturnCostAdjustment>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct), fees = await db.Set<FinanceFailedDeliveryFee>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct) };
    }

    public async Task<FinanceCostPolicy> AddCostPolicyAsync(CostPolicyCommand input, CancellationToken ct)
    {
        actor.RequireRole("Controller", "Admin"); var shop = Scope(input.ShopCode);
        if (!Enum.IsDefined(input.Method)) throw new ArgumentException("Unsupported costing method.");
        var group = Required(input.SkuGroup, "SkuGroup"); var period = Month(input.Period);
        var existing = await db.Set<FinanceCostPolicy>().SingleOrDefaultAsync(x => x.ShopCode == shop && x.SkuGroup == group && x.Period == period, ct);
        if (existing is not null)
        {
            if (existing.Method != input.Method) throw new ArgumentException("Cost policy is frozen for this group and period.");
            return existing;
        }
        var policy = new FinanceCostPolicy { ShopCode = shop, SkuGroup = group, Period = period, Method = input.Method, ApprovedBy = actor.UserName, Evidence = Required(input.Evidence, "Evidence") };
        db.Add(policy); Audit(shop, policy.Id, "CostPolicyApproved", policy.Evidence); await db.SaveChangesAsync(ct); return policy;
    }

    public async Task<FinanceCostLayer> AddCostLayerAsync(CostLayerCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin"); var shop = Scope(input.ShopCode);
        var layer = new FinanceCostLayer { ShopCode = shop, Sku = Required(input.Sku, "Sku"), Currency = Currency(input.Currency), SourceReceiptId = Required(input.SourceReceiptId, "SourceReceiptId"), Quantity = Positive(input.Quantity), UnitCost = Positive(input.UnitCost), ReceivedOn = ValidDate(input.ReceivedOn), Evidence = Required(input.Evidence, "Evidence") };
        var existing = await db.Set<FinanceCostLayer>().SingleOrDefaultAsync(x => x.ShopCode == shop && x.Sku == layer.Sku && x.SourceReceiptId == layer.SourceReceiptId, ct);
        if (existing is not null)
        {
            if (existing.Quantity != layer.Quantity || existing.UnitCost != layer.UnitCost || existing.Currency != layer.Currency || existing.ReceivedOn != layer.ReceivedOn) throw new ArgumentException("Receipt source ID conflicts with existing cost layer.");
            return existing;
        }
        db.Add(layer); Audit(shop, layer.Id, "CostLayerImported", layer.Evidence); await db.SaveChangesAsync(ct); return layer;
    }

    public async Task<FinanceCostAssessment> AssessCostAsync(AssessCostCommand input, CancellationToken ct)
    {
        actor.RequireRole("Controller", "Admin");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var order = await OrderAsync(input.OrderLineId, ct); var date = ValidDate(input.PostedOn);
        var evidence = Required(input.Evidence, "Evidence");
        var prior = await db.Set<FinanceCostAssessment>().Include(x => x.Allocations).Where(x => x.OrderLineId == order.Id).ToListAsync(ct);
        if (prior.Any(x => x.Status == FinanceCostStatus.Final)) throw new ArgumentException("Final cost is already recorded; use an approved adjustment workflow.");
        if (input.IsTemporary && prior.Count > 0) throw new ArgumentException("Temporary cost is already recorded.");
        await EnsureOpenAsync(order.ShopCode, date, ct);
        var policyPeriod = Month(order.OccurredOn);
        var policy = await db.Set<FinanceCostPolicy>().SingleOrDefaultAsync(x => x.ShopCode == order.ShopCode && x.SkuGroup == order.SkuGroup && x.Period == policyPeriod, ct)
            ?? throw new ArgumentException("Approved cost policy is required for the sale period.");
        var assessmentId = Guid.NewGuid(); var allocations = new List<FinanceCostAllocation>(); decimal cost;
        if (input.IsTemporary) cost = Money(input.EstimatedAmount ?? throw new ArgumentException("EstimatedAmount is required for temporary cost."));
        else
        {
            var candidates = await db.Set<FinanceCostLayer>().Where(x => x.ShopCode == order.ShopCode && x.Sku == order.Sku && x.Currency == order.Currency && x.ReceivedOn <= order.OccurredOn).ToListAsync(ct);
            var layers = candidates.Where(x => x.Quantity > x.UsedQuantity).OrderBy(x => x.ReceivedOn).ThenBy(x => x.SourceReceiptId, StringComparer.Ordinal).ToList();
            var available = layers.Sum(x => x.Quantity - x.UsedQuantity);
            if (available < order.Quantity) throw new ArgumentException("Insufficient evidenced cost layers for final costing.");
            var average = layers.Sum(x => (x.Quantity - x.UsedQuantity) * x.UnitCost) / available;
            var remaining = order.Quantity;
            foreach (var layer in layers)
            {
                if (remaining == 0) break;
                var qty = policy.Method == CostMethod.Fifo ? Math.Min(remaining, layer.Quantity - layer.UsedQuantity)
                    : layer == layers[^1] ? remaining : Math.Min(remaining, order.Quantity * (layer.Quantity - layer.UsedQuantity) / available);
                layer.UsedQuantity += qty; remaining -= qty;
                allocations.Add(new FinanceCostAllocation { FinanceCostAssessmentId = assessmentId, CostLayerId = layer.Id, Quantity = qty, Amount = qty * (policy.Method == CostMethod.Fifo ? layer.UnitCost : average) });
            }
            cost = decimal.Round(allocations.Sum(x => x.Amount), 2, MidpointRounding.AwayFromZero);
            // Stable rounding residual belongs to the last source allocation.
            if (allocations.Count > 0)
            {
                var last = allocations[^1]; var residual = cost - allocations.Sum(x => x.Amount);
                allocations[^1] = new FinanceCostAllocation { Id = last.Id, FinanceCostAssessmentId = assessmentId, CostLayerId = last.CostLayerId, Quantity = last.Quantity, Amount = last.Amount + residual };
            }
        }
        var temporary = prior.SingleOrDefault(); var delta = cost - (temporary?.Amount ?? 0);
        JournalEntry? journal = delta == 0 ? null : Journal(order.ShopCode, order.Currency, date, "CostAssessment", assessmentId.ToString(), delta > 0 ? "632" : "156", delta > 0 ? "156" : "632", Math.Abs(delta));
        var assessment = new FinanceCostAssessment { Id = assessmentId, OrderLineId = order.Id, ShopCode = order.ShopCode, Status = input.IsTemporary ? FinanceCostStatus.Temporary : FinanceCostStatus.Final, Amount = cost, Adjustment = delta, ReplacesAssessmentId = temporary?.Id, JournalEntryId = journal?.Id, PolicyId = policy.Id, Evidence = evidence, AssessedBy = actor.UserName, PostedOn = date, Allocations = allocations };
        if (journal is not null) db.Add(journal);
        db.Add(assessment); Audit(order.ShopCode, assessment.Id, input.IsTemporary ? "TemporaryCostRecorded" : "FinalCostRecorded", evidence);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return assessment;
    }

    public async Task<object> CostsAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        return new { policies = await db.Set<FinanceCostPolicy>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct), layers = await db.Set<FinanceCostLayer>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct), assessments = await db.Set<FinanceCostAssessment>().AsNoTracking().Include(x => x.Allocations).Where(x => x.ShopCode == shop).ToListAsync(ct) };
    }

    public async Task<object> AddDocumentAsync(DocumentCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin"); var shop = Scope(input.ShopCode);
        var externalId = Required(input.ExternalId, "ExternalId"); var kind = Required(input.Kind, "Kind");
        if (kind is not ("Invoice" or "CreditNote" or "SupportingDocument" or "ReturnEvidence")) throw new ArgumentException("Unsupported document kind.");
        if (!await db.Set<FinanceOrderLine>().AnyAsync(x => x.Id == input.SourceId && x.ShopCode == shop, ct)
            && !await db.Set<RefundConfirmation>().AnyAsync(x => x.Id == input.SourceId && x.ShopCode == shop, ct)
            && !await db.JournalEntries.AnyAsync(x => x.Id == input.SourceId && x.ShopCode == shop, ct)) throw new ArgumentException("Document source must exist in the selected shop.");
        byte[] bytes;
        try { bytes = Convert.FromBase64String(input.ContentBase64 ?? ""); } catch (FormatException) { throw new ArgumentException("Invalid document base64 content."); }
        if (bytes.Length is < 1 or > 5_000_000) throw new ArgumentException("Document size must be between 1 byte and 5 MB.");
        var hash = Convert.ToHexString(SHA256.HashData(bytes));
        var fileName = Required(input.FileName, "FileName");
        if (fileName.Length > 200 || fileName.IndexOfAny(['/', '\\']) >= 0) throw new ArgumentException("Use a filename without a directory path (maximum 200 characters).");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var document = await db.Set<FinanceDocument>().Include(x => x.Revisions).SingleOrDefaultAsync(x => x.ShopCode == shop && x.ExternalId == externalId, ct);
        if (document is null) { document = new FinanceDocument { ShopCode = shop, ExternalId = externalId, Kind = kind, SourceId = input.SourceId }; db.Add(document); }
        else if (document.SourceId != input.SourceId || document.Kind != kind) throw new ArgumentException("Document identity cannot be reassigned to a different source or kind.");
        var duplicate = document.Revisions.FirstOrDefault(x => x.Sha256 == hash);
        if (duplicate is not null) return DocumentResult(document, duplicate);
        var revision = new FinanceDocumentRevision { FinanceDocumentId = document.Id, Version = document.Revisions.Count + 1, FileName = fileName, Content = bytes, Sha256 = hash, Reason = Required(input.Reason, "Revision reason"), UploadedBy = actor.UserName };
        document.Revisions.Add(revision); Audit(shop, document.Id, "DocumentRevisionUploaded", hash);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return DocumentResult(document, revision);
    }

    public async Task<object> ReviewDocumentAsync(Guid revisionId, ReviewDocumentCommand command, CancellationToken ct)
    {
        actor.RequireRole("Checker", "Controller", "Admin");
        var revision = await db.Set<FinanceDocumentRevision>().SingleOrDefaultAsync(x => x.Id == revisionId, ct) ?? throw new ArgumentException("Document revision not found.");
        var document = await db.Set<FinanceDocument>().SingleAsync(x => x.Id == revision.FinanceDocumentId, ct); Scope(document.ShopCode);
        if (revision.UploadedBy.Equals(actor.UserName, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Uploader cannot review their own document.");
        if (revision.Status != FinanceDocumentStatus.PendingReview) throw new ArgumentException("A reviewed revision is immutable. Upload a new version.");
        revision.Status = command.Approve ? FinanceDocumentStatus.Verified : FinanceDocumentStatus.Rejected;
        revision.ReviewedBy = actor.UserName; revision.ReviewNote = Required(command.Note, "Review note"); revision.ReviewedAt = DateTimeOffset.UtcNow;
        Audit(document.ShopCode, document.Id, command.Approve ? "DocumentVerified" : "DocumentRejected", revision.ReviewNote);
        await db.SaveChangesAsync(ct); return DocumentResult(document, revision);
    }

    public async Task<object> DocumentsAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        var documents = await db.Set<FinanceDocument>().AsNoTracking().Include(x => x.Revisions).Where(x => x.ShopCode == shop).ToListAsync(ct);
        return documents.Select(x => new { x.Id, x.ExternalId, x.ShopCode, x.Kind, x.SourceId, revisions = x.Revisions.OrderBy(r => r.Version).Select(r => DocumentResult(x, r)) });
    }

    public async Task<(byte[] Content, string FileName)> DownloadAsync(Guid revisionId, CancellationToken ct)
    {
        var revision = await db.Set<FinanceDocumentRevision>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == revisionId, ct) ?? throw new ArgumentException("Document revision not found.");
        var document = await db.Set<FinanceDocument>().AsNoTracking().SingleAsync(x => x.Id == revision.FinanceDocumentId, ct); Scope(document.ShopCode);
        return (revision.Content, revision.FileName);
    }

    public async Task<IReadOnlyList<FinanceExtensionAudit>> AuditAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        var events = await db.Set<FinanceExtensionAudit>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct);
        return events.OrderBy(x => x.OccurredAt).ThenBy(x => x.Id).ToList();
    }

    public async Task<PayoutAccountChange> ProposePayoutAccountAsync(ProposePayoutAccountCommand input, CancellationToken ct)
    {
        actor.RequireRole("Maker", "Admin"); var shop = Scope(input.ShopCode);
        var sourceId = Required(input.SourceRequestId, "SourceRequestId");
        var reference = Required(input.NewAccountReference, "NewAccountReference");
        var evidence = Required(input.IndependentChannelEvidence, "Independent channel verification evidence");
        if (reference.Length > 200 || evidence.Length > 4000) throw new ArgumentException("Account reference or verification evidence is too long.");
        var existing = await db.Set<PayoutAccountChange>().SingleOrDefaultAsync(x => x.ShopCode == shop && x.SourceRequestId == sourceId, ct);
        if (existing is not null)
        {
            if (existing.NewAccountReference != reference || existing.IndependentChannelEvidence != evidence) throw new ArgumentException("Account change request ID conflicts with existing data.");
            return existing;
        }
        var previous = await db.Set<PayoutReceivingAccount>().SingleOrDefaultAsync(x => x.ShopCode == shop && x.DeactivatedAt == null, ct);
        if (previous?.AccountReference == reference) throw new ArgumentException("The proposed account is already active.");
        var change = new PayoutAccountChange { ShopCode = shop, SourceRequestId = sourceId, PreviousAccountId = previous?.Id, NewAccountReference = reference, IndependentChannelEvidence = evidence, Reason = Required(input.Reason, "Reason"), ProposedBy = actor.UserName };
        db.Add(change); Audit(shop, change.Id, "PayoutAccountChangeProposed", evidence);
        await db.SaveChangesAsync(ct); return change;
    }

    public async Task<PayoutAccountChange> ApprovePayoutAccountAsync(Guid id, ApprovePayoutAccountCommand input, CancellationToken ct)
    {
        actor.RequireApproval(0, "PayoutAccountChange");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var change = await db.Set<PayoutAccountChange>().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new ArgumentException("Payout account change not found."); Scope(change.ShopCode);
        if (change.ProposedBy.Equals(actor.UserName, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Proposer cannot approve their own payout account change.");
        if (change.Status != PayoutAccountChangeStatus.PendingApproval) throw new ArgumentException("Payout account change is not awaiting approval.");
        var evidence = Required(input.Evidence, "Independent review evidence");
        var previous = await db.Set<PayoutReceivingAccount>().SingleOrDefaultAsync(x => x.ShopCode == change.ShopCode && x.DeactivatedAt == null, ct);
        if (previous?.Id != change.PreviousAccountId) throw new ArgumentException("The active account changed after this proposal. Submit a new request against the current account.");
        var now = DateTimeOffset.UtcNow;
        if (previous is not null)
        {
            previous.DeactivatedAt = now;
            // Release the active-row uniqueness constraint before inserting the new version, inside one transaction.
            await db.SaveChangesAsync(ct);
        }
        change.Status = PayoutAccountChangeStatus.Approved; change.ApprovedBy = actor.UserName; change.ApprovedAt = now; change.ApprovalEvidence = evidence;
        db.Add(new PayoutReceivingAccount { ShopCode = change.ShopCode, AccountReference = change.NewAccountReference, ApprovedChangeId = change.Id, ActivatedAt = now });
        Audit(change.ShopCode, change.Id, "PayoutAccountChangeApproved", evidence);
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return change;
    }

    public async Task<object> PayoutAccountsAsync(string shopCode, CancellationToken ct)
    {
        var shop = Scope(shopCode);
        var accounts = await db.Set<PayoutReceivingAccount>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct);
        var changes = await db.Set<PayoutAccountChange>().AsNoTracking().Where(x => x.ShopCode == shop).ToListAsync(ct);
        return new { active = accounts.SingleOrDefault(x => x.DeactivatedAt == null), history = accounts.OrderBy(x => x.ActivatedAt), changes = changes.OrderBy(x => x.ProposedAt), verificationStatus = "Recorded evidence was reviewed; no external bank or channel validation was performed." };
    }

    private static object DocumentResult(FinanceDocument document, FinanceDocumentRevision revision) => new { documentId = document.Id, revisionId = revision.Id, document.ExternalId, document.ShopCode, document.Kind, document.SourceId, revision.Version, revision.FileName, revision.Sha256, revision.Status, revision.Reason, revision.UploadedBy, revision.UploadedAt, revision.ReviewedBy, revision.ReviewedAt, revision.ReviewNote, complianceStatus = "Evidence review only; legal invoice compliance has not been assessed." };
    private async Task<FinanceOrderLine> OrderAsync(Guid id, CancellationToken ct) { var order = await db.Set<FinanceOrderLine>().SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new ArgumentException("Order line not found."); Scope(order.ShopCode); return order; }
    private async Task EnsureOpenAsync(string shop, DateOnly date, CancellationToken ct) { var period = Month(date); if (await db.ClosedPeriods.AnyAsync(x => x.ShopCode == shop && x.Period == period, ct)) throw new ArgumentException("Posting period is closed."); }
    private JournalEntry Journal(string shop, string currency, DateOnly date, string sourceType, string sourceId, string debitAccount, string creditAccount, decimal amount)
    {
        var id = Guid.NewGuid();
        return new JournalEntry { Id = id, ReferenceType = sourceType, ReferenceId = sourceId, ShopCode = shop, Currency = currency, Period = Month(date), PostedBy = actor.UserName, Lines = [new JournalLine { JournalEntryId = id, AccountCode = debitAccount, Debit = amount }, new JournalLine { JournalEntryId = id, AccountCode = creditAccount, Credit = amount }] };
    }
    private void Audit(string shop, Guid source, string action, string evidence) => db.Add(new FinanceExtensionAudit { ShopCode = shop, SourceId = source, Action = action, Actor = actor.UserName, Evidence = evidence });
    private string Scope(string shop) { shop = Required(shop, "ShopCode").ToUpperInvariant(); actor.RequireShop(shop); return shop; }
    private static DateOnly Month(DateOnly date) { ValidDate(date); return new DateOnly(date.Year, date.Month, 1); }
    private static DateOnly ValidDate(DateOnly date) => date == default ? throw new ArgumentException("A valid date is required.") : date;
    private static string Required(string? value, string field) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException($"{field} is required.") : value.Trim();
    private static decimal Positive(decimal value) => value <= 0 ? throw new ArgumentException("Quantity and money must be greater than zero.") : value;
    private static decimal Money(decimal value) { Positive(value); if (decimal.Round(value, 2) != value) throw new ArgumentException("Money supports at most two fractional digits."); return value; }
    private static string Currency(string value) { var currency = Required(value, "Currency").ToUpperInvariant(); if (currency.Length != 3 || currency.Any(c => c < 'A' || c > 'Z')) throw new ArgumentException("Use a three-letter currency code."); return currency; }
}
