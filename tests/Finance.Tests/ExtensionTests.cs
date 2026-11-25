using Finance.Application.Security;
using Finance.Domain.Extensions;
using Finance.Domain.Ledger;
using Finance.Infrastructure.Extensions;
using Finance.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Finance.Tests;

public sealed class ExtensionTests
{
    [Fact]
    public async Task ExternalRefund_IsIdempotent_AndCannotExceedRemainingOrderAmount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync();
        var command = new RefundCommand(order.Id, "REF-1", 600, new(2026, 12, 12), "marketplace confirmation");
        var first = await fixture.Service.ConfirmExternalRefundAsync(command, default);
        var retry = await fixture.Service.ConfirmExternalRefundAsync(command, default);
        Assert.Equal(first.Id, retry.Id);
        Assert.Single(await fixture.Db.Set<RefundConfirmation>().ToListAsync());
        Assert.Single(await fixture.Db.JournalEntries.ToListAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ConfirmExternalRefundAsync(command with { SourceRefundId = "REF-2", Amount = 401 }, default));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ConfirmExternalRefundAsync(command with { Amount = 500 }, default));
        var last = await fixture.Service.ConfirmExternalRefundAsync(command with { SourceRefundId = "REF-2", Amount = 400 }, default);
        Assert.Equal(400, last.Amount);
        var journals = await fixture.Db.JournalEntries.Include(x => x.Lines).ToListAsync();
        Assert.All(journals, entry => Assert.Equal(entry.Lines.Sum(x => x.Debit), entry.Lines.Sum(x => x.Credit)));
    }

    [Fact]
    public async Task ExternalRefund_CannotPostInClosedMonth()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync();
        fixture.Db.ClosedPeriods.Add(new ClosedPeriod { ShopCode = "SHOP-A", Period = new(2026, 12, 1), ClosedBy = "controller" });
        await fixture.Db.SaveChangesAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ConfirmExternalRefundAsync(new(order.Id, "REF-1", 100, new(2026, 12, 25), "confirmation"), default));
        Assert.Empty(await fixture.Db.Set<RefundConfirmation>().ToListAsync());
    }

    [Fact]
    public async Task TemporaryCost_FinalFifoCostPostsOnlyDifference()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync(quantity: 2);
        await fixture.Service.AddCostPolicyAsync(new("SHOP-A", "fashion", new(2026, 12, 20), CostMethod.Fifo, "approved policy"), default);
        await fixture.Service.AddCostLayerAsync(new("SHOP-A", "SKU-1", "VND", "GRN-1", 1, 100, new(2026, 12, 1), "receipt A"), default);
        await fixture.Service.AddCostLayerAsync(new("SHOP-A", "SKU-1", "VND", "GRN-2", 2, 200, new(2026, 12, 2), "receipt B"), default);
        var temporary = await fixture.Service.AssessCostAsync(new(order.Id, true, 250, new(2026, 12, 12), "estimate"), default);
        var final = await fixture.Service.AssessCostAsync(new(order.Id, false, null, new(2026, 12, 13), "verified layers"), default);
        Assert.Equal(FinanceCostStatus.Temporary, temporary.Status);
        Assert.Equal(300, final.Amount);
        Assert.Equal(50, final.Adjustment);
        Assert.Equal(temporary.Id, final.ReplacesAssessmentId);
        Assert.Equal(2, final.Allocations.Sum(x => x.Quantity));
        Assert.Equal(300, final.Allocations.Sum(x => x.Amount));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.AssessCostAsync(new(order.Id, false, null, new(2026, 12, 13), "retry"), default));
        Assert.Equal(2, await fixture.Db.Set<FinanceCostAssessment>().CountAsync());
    }

    [Fact]
    public async Task WeightedAverageCost_LeavesConsistentRemainingFinancialQuantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var first = await fixture.OrderAsync("LINE-1", 1);
        var second = await fixture.OrderAsync("LINE-2", 1);
        await fixture.Service.AddCostPolicyAsync(new("SHOP-A", "fashion", new(2026, 12, 1), CostMethod.WeightedAverage, "policy"), default);
        await fixture.Service.AddCostLayerAsync(new("SHOP-A", "SKU-1", "VND", "GRN-1", 1, 100, new(2026, 12, 1), "receipt"), default);
        await fixture.Service.AddCostLayerAsync(new("SHOP-A", "SKU-1", "VND", "GRN-2", 1, 300, new(2026, 12, 2), "receipt"), default);
        var a = await fixture.Service.AssessCostAsync(new(first.Id, false, null, new(2026, 12, 12), "verified"), default);
        var b = await fixture.Service.AssessCostAsync(new(second.Id, false, null, new(2026, 12, 12), "verified"), default);
        Assert.Equal(200, a.Amount); Assert.Equal(200, b.Amount);
        Assert.All(await fixture.Db.Set<FinanceCostLayer>().ToListAsync(), layer => Assert.Equal(layer.Quantity, layer.UsedQuantity));
    }

    [Fact]
    public async Task CostPolicy_IsFrozenAndInsufficientLayersCannotBecomeFinal()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync();
        var command = new CostPolicyCommand("SHOP-A", "fashion", new(2026, 12, 1), CostMethod.Fifo, "approved");
        await fixture.Service.AddCostPolicyAsync(command, default);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.AddCostPolicyAsync(command with { Method = CostMethod.WeightedAverage }, default));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.AssessCostAsync(new(order.Id, false, null, new(2026, 12, 12), "verified"), default));
        Assert.Empty(await fixture.Db.Set<FinanceCostAssessment>().ToListAsync());
    }

    [Fact]
    public async Task DocumentReview_RequiresIndependentReviewerAndRetainsVersions()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync();
        var input = new DocumentCommand("SHOP-A", "INV-1", "Invoice", order.Id, "invoice.pdf", Convert.ToBase64String([1, 2, 3]), "original invoice");
        await fixture.Service.AddDocumentAsync(input, default);
        await fixture.Service.AddDocumentAsync(input, default);
        var revision = await fixture.Db.Set<FinanceDocumentRevision>().SingleAsync();
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ReviewDocumentAsync(revision.Id, new(true, "verified"), default));
        fixture.Actor.Name = "independent-checker";
        await fixture.Service.ReviewDocumentAsync(revision.Id, new(true, "verified"), default);
        Assert.Equal(FinanceDocumentStatus.Verified, revision.Status);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ReviewDocumentAsync(revision.Id, new(false, "change"), default));
        fixture.Actor.Name = "maker";
        await fixture.Service.AddDocumentAsync(input with { ContentBase64 = Convert.ToBase64String([4, 5, 6]), Reason = "corrected document" }, default);
        var versions = await fixture.Db.Set<FinanceDocumentRevision>().OrderBy(x => x.Version).ToListAsync();
        Assert.Equal(2, versions.Count);
        Assert.Equal(FinanceDocumentStatus.Verified, versions[0].Status);
        Assert.Equal(FinanceDocumentStatus.PendingReview, versions[1].Status);
        Assert.NotEqual(versions[0].Sha256, versions[1].Sha256);
    }

    [Fact]
    public async Task ScopeFiltersAndPermissions_BlockOtherShopAndUnprivilegedWrites()
    {
        await using var fixture = await Fixture.CreateAsync();
        await fixture.OrderAsync();
        fixture.Actor.ShopScope = ["SHOP-B"];
        Assert.Empty(await fixture.Db.Set<FinanceOrderLine>().AsNoTracking().ToListAsync());
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.OrdersAsync("SHOP-A", default));
        fixture.Actor.ShopScope = ["SHOP-A"];
        fixture.Actor.RoleScope = ["Viewer"];
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.ConfirmExternalRefundAsync(new(Guid.NewGuid(), "REF-1", 10, new(2026, 12, 12), "evidence"), default));
    }

    [Fact]
    public async Task RefundRequest_RequiresIndependentApprovalAndConfirmationIsNotPaymentExecution()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.Service.AddOrderAsync(new("D2C-LINE", "D2C-ORDER", "SHOP-A", "ENTITY-A", "D2C", "VND", "SKU-1", "fashion", 1, 1000, new(2026, 12, 12), "OMS snapshot"), default);
        var request = await fixture.Service.RequestRefundAsync(new(order.Id, "REQ-1", 500, "return", "QC case"), default);
        Assert.Equal(FinanceRefundRequestStatus.Requested, request.Status);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ApproveRefundAsync(request.Id, new("approval"), default));
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ConfirmExternalRefundAsync(new(order.Id, "RF-1", 500, new(2026, 12, 13), "bank evidence"), default));
        Assert.Empty(await fixture.Db.JournalEntries.ToListAsync());
        fixture.Actor.Name = "checker";
        await fixture.Service.ApproveRefundAsync(request.Id, new("verified approval"), default);
        Assert.Equal(FinanceRefundRequestStatus.Approved, request.Status);
        Assert.Empty(await fixture.Db.JournalEntries.ToListAsync());
        fixture.Actor.Name = "maker";
        await fixture.Service.ConfirmExternalRefundAsync(new(order.Id, "RF-1", 500, new(2026, 12, 13), "external bank confirmation", request.Id), default);
        Assert.Equal(FinanceRefundRequestStatus.Confirmed, request.Status);
        Assert.Single(await fixture.Db.JournalEntries.ToListAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ConfirmExternalRefundAsync(new(order.Id, "RF-2", 500, new(2026, 12, 13), "duplicate approval reuse", request.Id), default));
    }

    [Fact]
    public async Task ReturnQc_ReversesFinalCostOnce_AndCapsCumulativeQuantity()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync(quantity: 2);
        await fixture.Service.AddCostPolicyAsync(new("SHOP-A", "fashion", new(2026, 12, 1), CostMethod.Fifo, "policy"), default);
        await fixture.Service.AddCostLayerAsync(new("SHOP-A", "SKU-1", "VND", "GRN-1", 2, 150, new(2026, 12, 1), "receipt"), default);
        var final = await fixture.Service.AssessCostAsync(new(order.Id, false, null, new(2026, 12, 12), "verified"), default);
        var command = new ReturnCostCommand(order.Id, "QC-1", ReturnCostDisposition.Restock, 1, new(2026, 12, 13), "QC evidence: accepted restock");
        var adjustment = await fixture.Service.RecordReturnCostAsync(command, default);
        var retry = await fixture.Service.RecordReturnCostAsync(command, default);
        Assert.Equal(adjustment.Id, retry.Id); Assert.Equal(150, adjustment.Amount); Assert.Equal(final.Id, adjustment.FinalAssessmentId);
        var scrap = await fixture.Service.RecordReturnCostAsync(command with { SourceQcId = "QC-2", Disposition = ReturnCostDisposition.Scrap }, default);
        Assert.Equal(0, scrap.Amount); Assert.Null(scrap.JournalEntryId);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.RecordReturnCostAsync(command with { SourceQcId = "QC-3" }, default));
        Assert.Equal(2, await fixture.Db.JournalEntries.CountAsync());
        Assert.Empty(await fixture.Db.Set<RefundConfirmation>().ToListAsync());
    }

    [Fact]
    public async Task FailedDeliveryFee_DeduplicatesEventAndMatchesExistingStatement()
    {
        await using var fixture = await Fixture.CreateAsync();
        var sourceLine = new Finance.Domain.Imports.ImportLine { ExternalId = "FEE-STATEMENT", Amount = -100, OccurredOn = new(2026, 12, 12), BusinessType = "ShippingFee", Status = Finance.Domain.Imports.ImportLineStatus.Valid, RawData = "FEE-STATEMENT,-100" };
        fixture.Db.ImportBatches.Add(new Finance.Domain.Imports.ImportBatch { SourceType = Finance.Domain.Imports.ImportSourceType.MarketplaceStatement, ShopCode = "SHOP-A", Currency = "VND", Period = new(2026, 12, 1), Checksum = "TEST-FEE", Lines = [sourceLine] });
        await fixture.Db.SaveChangesAsync();
        var linked = await fixture.Service.RecordFailedDeliveryFeeAsync(new("SHOP-A", "FEE-STATEMENT", "PACKAGE-1", "VND", 100, new(2026, 12, 12), "statement fee"), default);
        Assert.Equal(sourceLine.Id, linked.ImportLineId); Assert.Null(linked.JournalEntryId);
        var command = new FailedDeliveryFeeCommand("SHOP-A", "FEE-NEW", "PACKAGE-2", "VND", 150, new(2026, 12, 12), "carrier invoice");
        var fee = await fixture.Service.RecordFailedDeliveryFeeAsync(command, default);
        var retry = await fixture.Service.RecordFailedDeliveryFeeAsync(command, default);
        Assert.Equal(fee.Id, retry.Id);
        Assert.Single(await fixture.Db.JournalEntries.ToListAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.RecordFailedDeliveryFeeAsync(command with { Amount = 200 }, default));
    }

    [Fact]
    public async Task PayoutAccountChange_RequiresIndependentApprovalAndPreservesPreviousAccount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var initial = await fixture.Service.ProposePayoutAccountAsync(new("SHOP-A", "BANK-CHANGE-1", "bank-token-old", "independent Seller Center call evidence", "initial setup"), default);
        Assert.Empty(await fixture.Db.Set<PayoutReceivingAccount>().ToListAsync());
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ApprovePayoutAccountAsync(initial.Id, new("review"), default));
        fixture.Actor.Name = "checker";
        await fixture.Service.ApprovePayoutAccountAsync(initial.Id, new("verified independent channel evidence"), default);
        Assert.Equal("PayoutAccountChange", fixture.Actor.LastApprovalOperation);
        fixture.Actor.Name = "maker";
        var proposal = await fixture.Service.ProposePayoutAccountAsync(new("SHOP-A", "BANK-CHANGE-2", "bank-token-new", "independent channel verification 2", "bank changed"), default);
        var beforeApproval = await fixture.Db.Set<PayoutReceivingAccount>().SingleAsync(x => x.DeactivatedAt == null);
        Assert.Equal("bank-token-old", beforeApproval.AccountReference);
        Assert.Equal(beforeApproval.Id, proposal.PreviousAccountId);
        fixture.Actor.Name = "checker";
        await fixture.Service.ApprovePayoutAccountAsync(proposal.Id, new("second independent review"), default);
        var accounts = await fixture.Db.Set<PayoutReceivingAccount>().ToListAsync();
        Assert.Equal(2, accounts.Count);
        Assert.NotNull(accounts.Single(x => x.AccountReference == "bank-token-old").DeactivatedAt);
        Assert.Equal("bank-token-new", accounts.Single(x => x.DeactivatedAt == null).AccountReference);
        Assert.Equal(4, await fixture.Db.Set<FinanceExtensionAudit>().CountAsync(x => x.Action.StartsWith("PayoutAccountChange")));
        Assert.Empty(await fixture.Db.JournalEntries.ToListAsync());
    }

    [Fact]
    public async Task PayoutAccountStaleProposalCannotReplaceNewlyApprovedAccount()
    {
        await using var fixture = await Fixture.CreateAsync();
        var proposalA = await fixture.Service.ProposePayoutAccountAsync(new("SHOP-A", "CHANGE-A", "bank-token-A", "verification A", "setup A"), default);
        var proposalB = await fixture.Service.ProposePayoutAccountAsync(new("SHOP-A", "CHANGE-B", "bank-token-B", "verification B", "setup B"), default);
        fixture.Actor.Name = "checker";
        await fixture.Service.ApprovePayoutAccountAsync(proposalA.Id, new("review A"), default);
        await Assert.ThrowsAsync<ArgumentException>(() => fixture.Service.ApprovePayoutAccountAsync(proposalB.Id, new("review B"), default));
        var active = await fixture.Db.Set<PayoutReceivingAccount>().SingleAsync(x => x.DeactivatedAt == null);
        Assert.Equal("bank-token-A", active.AccountReference);
        Assert.Equal(PayoutAccountChangeStatus.PendingApproval, proposalB.Status);
    }

    [Fact]
    public async Task RefundApprovalChecksDelegatedAmountLimit()
    {
        await using var fixture = await Fixture.CreateAsync();
        var order = await fixture.OrderAsync();
        var request = await fixture.Service.RequestRefundAsync(new(order.Id, "REQ-LIMIT", 500, "return", "QC evidence"), default);
        fixture.Actor.Name = "checker"; fixture.Actor.ApprovalLimit = 100;
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => fixture.Service.ApproveRefundAsync(request.Id, new("review"), default));
        Assert.Equal(FinanceRefundRequestStatus.Requested, request.Status);
        Assert.Equal("Refund", fixture.Actor.LastApprovalOperation);
    }

    private sealed class ExtensionActor : ICurrentActor
    {
        public string Name { get; set; } = "maker";
        public string UserName => Name;
        public IReadOnlyList<string> ShopScope { get; set; } = ["SHOP-A"];
        public IReadOnlyList<string> RoleScope { get; set; } = ["Maker", "Checker", "Controller", "Admin"];
        public IReadOnlyList<string> Shops => ShopScope;
        public IReadOnlyList<string> Roles => RoleScope;
        public decimal ApprovalLimit { get; set; } = decimal.MaxValue;
        public string? LastApprovalOperation { get; private set; }
        public void RequireShop(string shopCode) { if (!Shops.Contains(shopCode)) throw new UnauthorizedAccessException(); }
        public void RequireRole(params string[] roles) { if (!roles.Any(Roles.Contains)) throw new UnauthorizedAccessException(); }
        public void RequireApproval(decimal amount, string operation) { LastApprovalOperation = operation; RequireRole("Checker", "Controller"); if (amount > ApprovalLimit) throw new UnauthorizedAccessException(); }
    }

    private sealed class Fixture : IAsyncDisposable
    {
        private readonly SqliteConnection connection;
        public ExtensionActor Actor { get; }
        public FinanceDbContext Db { get; }
        public FinanceExtensionService Service { get; }
        private Fixture(SqliteConnection connection, FinanceDbContext db, ExtensionActor actor) { this.connection = connection; Db = db; Actor = actor; Service = new(db, actor); }
        public static async Task<Fixture> CreateAsync()
        {
            var connection = new SqliteConnection("Data Source=:memory:"); await connection.OpenAsync();
            var actor = new ExtensionActor(); var db = new FinanceDbContext(new DbContextOptionsBuilder<FinanceDbContext>().UseSqlite(connection).Options, actor);
            await db.Database.EnsureCreatedAsync(); return new(connection, db, actor);
        }
        public Task<FinanceOrderLine> OrderAsync(string externalId = "LINE-1", decimal quantity = 1) => Service.AddOrderAsync(new(externalId, "ORDER-1", "SHOP-A", "ENTITY-A", "TikTok", "VND", "SKU-1", "fashion", quantity, 1000, new(2026, 12, 12), "OMS snapshot"), default);
        public async ValueTask DisposeAsync() { await Db.DisposeAsync(); await connection.DisposeAsync(); }
    }
}
