using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore;
using Finance.Infrastructure.Persistence;

namespace Finance.Tests;

public sealed class FinanceFactory : WebApplicationFactory<Program>
{
    readonly string database = Path.Combine(Path.GetTempPath(), "finance-tests-" + Guid.NewGuid() + ".db");
    // Each factory owns a fresh file: tests never touch the developer database.
    protected override void ConfigureWebHost(IWebHostBuilder builder) => builder.UseEnvironment("Development").ConfigureServices(services =>
    {
        services.RemoveAll<DbContextOptions<FinanceDbContext>>();
        services.RemoveAll<FinanceDbContext>();
        services.AddDbContext<FinanceDbContext>(options => options.UseSqlite($"Data Source={database}"));
    });
}

public sealed class FinanceApiTests
{
    static HttpClient Client(FinanceFactory factory) { var client = factory.CreateClient(new() { AllowAutoRedirect = false }); client.DefaultRequestHeaders.Add("X-Finance-Client", "web"); return client; }
    static async Task Login(HttpClient client, string username) => (await client.PostAsJsonAsync("/api/auth/login", new { username, password = "FinanceDemo!2026" })).EnsureSuccessStatusCode();
    static async Task<JsonElement> Post(HttpClient client, string path, object payload) { var response = await client.PostAsJsonAsync(path, payload); Assert.True(response.IsSuccessStatusCode, await response.Content.ReadAsStringAsync()); return (await response.Content.ReadFromJsonAsync<JsonElement>()); }
    static object Batch(string id, decimal amount = 100) => new { sourceType = "MarketplaceStatement", shopCode = "TIKTOK-HCM", currency = "VND", period = "2026-12-01", importedBy = "spoofed", lines = new[] { new { externalId = id, amount, occurredOn = "2026-12-10", businessType = "Revenue", rawData = (string?)null } } };

    [Fact]
    public async Task Authentication_and_shop_scope_apply_to_reads_and_writes()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/finance/import-batches")).StatusCode);
        await Login(client, "maker");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/finance/ledger/periods/close", new { shopCode = "TIKTOK-HCM", period = "2026-12-01", closedBy = "controller" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync("/api/finance/import-batches", new { shopCode = "SHOPEE-HCM", sourceType = "MarketplaceStatement", currency = "VND", period = "2026-12-01", importedBy = "admin", lines = new[] { new { externalId = "x", amount = 10, occurredOn = "2026-12-01", businessType = "Revenue" } } })).StatusCode);
        await Login(client, "admin");
        await Post(client, "/api/finance/import-batches", new { shopCode = "SHOPEE-HCM", sourceType = "MarketplaceStatement", currency = "VND", period = "2026-12-01", importedBy = "admin", lines = new[] { new { externalId = "other-shop", amount = 10, occurredOn = "2026-12-01", businessType = "Revenue" } } });
        await Login(client, "maker");
        var me = await client.GetFromJsonAsync<JsonElement>("/api/auth/me");
        Assert.Equal("maker", me.GetProperty("username").GetString());
        var rows = await client.GetFromJsonAsync<JsonElement>("/api/finance/import-batches");
        Assert.True(rows.GetArrayLength() == 0, rows.ToString());
    }

    [Fact]
    public async Task Import_preview_is_readonly_and_duplicate_events_are_quarantined()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        await Post(client, "/api/finance/import-batches/preview", Batch("one"));
        Assert.Equal(0, (await client.GetFromJsonAsync<JsonElement>("/api/finance/import-batches")).GetArrayLength());
        var first = await Post(client, "/api/finance/import-batches", Batch("one"));
        var retry = await Post(client, "/api/finance/import-batches", Batch("one"));
        Assert.Equal(first.GetProperty("id").GetString(), retry.GetProperty("id").GetString());
        Assert.True(retry.GetProperty("isDuplicate").GetBoolean());
        var changedAmount = await Post(client, "/api/finance/import-batches", Batch("one", 200));
        Assert.Equal(1, changedAmount.GetProperty("quarantinedLines").GetInt32());
        var differentId = await Post(client, "/api/finance/import-batches", Batch("two"));
        Assert.NotEqual(first.GetProperty("id").GetString(), differentId.GetProperty("id").GetString());
    }

    [Fact]
    public async Task Journal_retry_is_idempotent_and_whole_month_is_closed()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        object Journal(string id, string period, decimal amount = 100) => new { shopCode = "TIKTOK-HCM", currency = "VND", referenceType = "Settlement", referenceId = id, period, postedBy = "spoofed", reason = "Verified adjustment", evidence = "approved-evidence", lines = new[] { new { accountCode = "112", debit = amount, credit = 0m }, new { accountCode = "511", debit = 0m, credit = amount } } };
        var entry = await Post(client, "/api/finance/ledger/journals", Journal("j1", "2026-12-15"));
        Assert.Equal(entry.GetProperty("id").GetString(), (await Post(client, "/api/finance/ledger/journals", Journal("j1", "2026-12-20"))).GetProperty("id").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/finance/ledger/journals", Journal("j1", "2026-12-15", 200))).StatusCode);
        await Login(client, "controller");
        (await client.PostAsJsonAsync("/api/finance/ledger/periods/close", new { shopCode = "TIKTOK-HCM", period = "2026-12-31", closedBy = "spoofed" })).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/finance/ledger/journals", Journal("j2", "2026-12-02"))).StatusCode);
        var reverse = await Post(client, $"/api/finance/ledger/journals/{entry.GetProperty("id").GetString()}/reverse", new { period = "2027-01-02", postedBy = "spoofed" });
        Assert.Equal(entry.GetProperty("id").GetString(), reverse.GetProperty("reversesJournalEntryId").GetString());
    }

    [Fact]
    public async Task WaitingExternal_needs_verified_outcome_and_real_checker()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        var entry = await Post(client, "/api/finance/exceptions", new { referenceType = "Statement", referenceId = "ex1", shopCode = "TIKTOK-HCM", reasonCode = "FEE", amount = 10, owner = "maker", dueOn = "2026-12-31", evidence = "source", createdBy = "fake" });
        var path = "/api/finance/exceptions/" + entry.GetProperty("id").GetString();
        await Post(client, path + "/wait-external", new { actor = "fake", note = "sent ticket", evidence = "ticket" });
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync(path + "/request-closure", new { actor = "fake", note = "sent", evidence = "ticket" })).StatusCode);
        await Post(client, path + "/request-closure", new { actor = "fake", note = "bank compensation verified", evidence = "bank-proof", outcome = "Verified" });
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync(path + "/approve-closure", new { actor = "checker", note = "yes", evidence = "review" })).StatusCode);
        await Login(client, "checker");
        var closed = await Post(client, path + "/approve-closure", new { actor = "fake", note = "yes", evidence = "review" });
        Assert.Equal("Closed", closed.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Unmapped_line_can_be_reclassified_after_mapping_added()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        var batch = await Post(client, "/api/finance/import-batches", Batch("mapping"));
        var path = $"/api/finance/classification/batches/{batch.GetProperty("id").GetString()}/classify";
        Assert.Equal(1, (await Post(client, path, new { effectiveOn = "2026-12-10", actor = "fake" })).GetProperty("unmapped").GetInt32());
        await Login(client, "admin");
        var mapping = new { sourceType = "MarketplaceStatement", sourceBusinessType = "Revenue", targetCategory = "Revenue", version = 1, effectiveFrom = "2026-01-01", effectiveTo = "2026-12-31", createdBy = "fake" };
        (await client.PostAsJsonAsync("/api/finance/classification/mappings", mapping)).EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/finance/classification/mappings", mapping)).StatusCode);
        Assert.Equal(1, (await Post(client, path, new { effectiveOn = "2030-01-01", actor = "fake" })).GetProperty("classified").GetInt32());
    }

    [Fact]
    public async Task Reconciliation_requires_statement_scope_and_cannot_reuse_bank_money()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        await Post(client, "/api/finance/import-batches", Batch("settlement", 100));
        var batch = (await client.GetFromJsonAsync<JsonElement>("/api/finance/import-batches"))[0];
        var lineId = batch.GetProperty("lines")[0].GetProperty("id").GetString();
        var order = await Post(client, "/api/finance/extensions/orders", new { externalId = "order-line-1", orderId = "order-1", shopCode = "TIKTOK-HCM", legalEntity = "ENTITY-1", channel = "TikTok", currency = "VND", sku = "SKU-1", skuGroup = "Fashion", quantity = 1, netAmount = 100, occurredOn = "2026-12-10", evidence = "OMS-delivered" });
        await client.PostAsJsonAsync("/api/finance/reconciliation/payouts", new { externalId = "p1", shopCode = "TIKTOK-HCM", currency = "VND", amount = 100, payoutOn = "2026-12-10" });
        await client.PostAsJsonAsync("/api/finance/reconciliation/payouts", new { externalId = "p2", shopCode = "TIKTOK-HCM", currency = "VND", amount = 100, payoutOn = "2026-12-10" });
        await client.PostAsJsonAsync("/api/finance/reconciliation/bank-transactions", new { externalId = "b1", shopCode = "TIKTOK-HCM", currency = "VND", amount = 100, creditedOn = "2026-12-11" });
        var payouts = await client.GetFromJsonAsync<JsonElement>("/api/finance/reconciliation/payouts");
        var banks = await client.GetFromJsonAsync<JsonElement>("/api/finance/reconciliation/bank-transactions");
        var firstPayout = payouts.EnumerateArray().First(p => p.GetProperty("externalId").GetString() == "p1").GetProperty("id").GetString();
        var secondPayout = payouts.EnumerateArray().First(p => p.GetProperty("externalId").GetString() == "p2").GetProperty("id").GetString();
        object Case(string? payoutId, int tolerance = 0) => new { payoutId, bankTransactionId = banks[0].GetProperty("id").GetString(), tolerance, evidence = "bank-credit-proof", proposedBy = "checker", reason = "Source payout reference verified", allocations = new[] { new { importLineId = lineId, amount = 100, orderLineId = order.GetProperty("id").GetString() } } };
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/finance/reconciliation/cases", Case(firstPayout, 100))).StatusCode);
        var candidate = await Post(client, "/api/finance/reconciliation/cases", Case(firstPayout));
        Assert.Equal("Candidate", candidate.GetProperty("status").GetString());
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/finance/reconciliation/cases", Case(secondPayout))).StatusCode);
        await Login(client, "checker");
        var matched = await Post(client, $"/api/finance/reconciliation/cases/{candidate.GetProperty("id").GetString()}/confirm", "maker");
        Assert.Equal("FullyMatched", matched.GetProperty("status").GetString());
        Assert.Equal("checker", matched.GetProperty("reviewedBy").GetString());
        Assert.Equal(HttpStatusCode.Forbidden, (await client.GetAsync("/api/finance/reports/journals.csv?shopCode=SHOPEE-HCM")).StatusCode);
        var report = await client.GetFromJsonAsync<JsonElement>("/api/finance/reports/ledger?shopCode=TIKTOK-HCM&currency=VND&period=2026-12-01");
        Assert.Equal(100, report.GetProperty("receivedCash").GetDecimal());
        Assert.Equal("Missing", report.GetProperty("sourceStatus").GetString());
    }

    [Fact]
    public async Task Source_switch_requires_independent_approval_and_blocks_old_writer()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "admin");
        var request = await Post(client, "/api/finance/sources/switch-requests", new { shopCode = "TIKTOK-HCM", targetMode = "Connected", expectedVersion = 1, checkpoint = "source-checkpoint-1", mapping = new Dictionary<string, string> { ["local-shop"] = "TIKTOK-HCM" } });
        var path = "/api/finance/sources/switch-requests/" + request.GetProperty("id").GetString() + "/approve";
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync(path, new { })).StatusCode);
        await Login(client, "checker");
        Assert.Equal("Connected", (await Post(client, path, new { })).GetProperty("mode").GetString());
        await Login(client, "maker");
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/finance/import-batches", Batch("old-writer"))).StatusCode);
    }

    [Fact]
    public async Task Concurrent_journal_posts_never_duplicate_financial_effect()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        var journal = new { shopCode = "TIKTOK-HCM", currency = "VND", referenceType = "Manual", referenceId = "concurrent-event", period = "2026-12-01", postedBy = "ignored", reason = "checked", evidence = "approved", lines = new[] { new { accountCode = "112", debit = 100, credit = 0 }, new { accountCode = "511", debit = 0, credit = 100 } } };
        var responses = await Task.WhenAll(client.PostAsJsonAsync("/api/finance/ledger/journals", journal), client.PostAsJsonAsync("/api/finance/ledger/journals", journal));
        Assert.All(responses, response => Assert.True(response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.Conflict));
        Assert.Equal(1, (await client.GetFromJsonAsync<JsonElement>("/api/finance/ledger/journals")).GetArrayLength());
    }

    [Fact]
    public async Task Checker_cannot_approve_beyond_monetary_limit()
    {
        await using var factory = new FinanceFactory(); using var client = Client(factory); await Login(client, "maker");
        (await client.PostAsJsonAsync("/api/finance/reconciliation/payouts", new { externalId = "high-payout", shopCode = "TIKTOK-HCM", currency = "VND", amount = 20_000_000, payoutOn = "2026-12-10" })).EnsureSuccessStatusCode();
        (await client.PostAsJsonAsync("/api/finance/reconciliation/bank-transactions", new { externalId = "high-bank", shopCode = "TIKTOK-HCM", currency = "VND", amount = 20_000_000, creditedOn = "2026-12-10" })).EnsureSuccessStatusCode();
        var payout = (await client.GetFromJsonAsync<JsonElement>("/api/finance/reconciliation/payouts"))[0];
        var bank = (await client.GetFromJsonAsync<JsonElement>("/api/finance/reconciliation/bank-transactions"))[0];
        var candidate = await Post(client, "/api/finance/reconciliation/cases", new { payoutId = payout.GetProperty("id").GetString(), bankTransactionId = bank.GetProperty("id").GetString(), tolerance = 0, evidence = "bank", proposedBy = "maker", reason = "payout source" });
        await Login(client, "checker");
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsJsonAsync($"/api/finance/reconciliation/cases/{candidate.GetProperty("id").GetString()}/confirm", "checker")).StatusCode);
    }
}
