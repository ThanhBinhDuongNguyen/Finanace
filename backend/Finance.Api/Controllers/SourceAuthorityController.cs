using System.Text.Json;
using Finance.Application.Security;
using Finance.Domain.Imports;
using Finance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.Api.Controllers;

public sealed record SwitchSourceInput(string ShopCode, string TargetMode, int ExpectedVersion, string Checkpoint, Dictionary<string, string> Mapping);

[ApiController, Route("api/finance/sources")]
public sealed class SourceAuthorityController(FinanceDbContext db, ICurrentActor actor) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(new { authorities = await db.Set<SourceAuthority>().ToListAsync(ct), requests = await db.Set<SourceSwitchRequest>().ToListAsync(ct) });
    [HttpPost("switch-requests")]
    public async Task<IActionResult> Propose(SwitchSourceInput input, CancellationToken ct)
    {
        actor.RequireRole("Admin"); actor.RequireShop(input.ShopCode);
        if (input.TargetMode is not ("Local" or "Connected") || string.IsNullOrWhiteSpace(input.Checkpoint) || input.Mapping is null || input.Mapping.Count == 0 || input.Mapping.Any(kv => string.IsNullOrWhiteSpace(kv.Key) || string.IsNullOrWhiteSpace(kv.Value))) throw new ArgumentException("Chuyển nguồn cần mode, checkpoint và mapping ID hợp lệ.");
        var shop = FinanceRules.Scope(input.ShopCode);
        var openWork = await db.FinanceExceptions.Where(e => e.ShopCode == shop && e.Status != Finance.Domain.Exceptions.FinanceExceptionStatus.Closed).Select(e => e.Id).ToListAsync(ct);
        var request = new SourceSwitchRequest { ShopCode = shop, TargetMode = input.TargetMode, ExpectedVersion = input.ExpectedVersion, ProposedBy = actor.UserName, Checkpoint = input.Checkpoint.Trim(), MappingManifest = JsonSerializer.Serialize(input.Mapping), OpenWorkManifest = JsonSerializer.Serialize(openWork) };
        db.Add(request); await db.SaveChangesAsync(ct); return Ok(request);
    }
    [HttpPost("switch-requests/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        actor.RequireRole("Checker");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var request = await db.Set<SourceSwitchRequest>().SingleOrDefaultAsync(r => r.Id == id, ct) ?? throw new ArgumentException("Không tìm thấy yêu cầu.");
        if (request.ApprovedBy is not null || request.ProposedBy == actor.UserName) throw new InvalidOperationException("Yêu cầu đã duyệt hoặc không độc lập.");
        var authority = await db.Set<SourceAuthority>().SingleOrDefaultAsync(a => a.ShopCode == request.ShopCode, ct);
        if (request.ExpectedVersion != (authority?.Version ?? 1)) throw new InvalidOperationException("Checkpoint đã cũ, cần lập yêu cầu mới.");
        if (authority?.Mode == "Connected" && request.TargetMode == "Local") throw new InvalidOperationException("Rollback về Local cần quy trình chuyển giao riêng sau giao dịch mới.");
        if (authority is null) { authority = new SourceAuthority { ShopCode = request.ShopCode }; db.Add(authority); }
        authority.Mode = request.TargetMode; authority.Version++; authority.ApprovedBy = actor.UserName; request.ApprovedBy = actor.UserName;
        await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return Ok(authority);
    }
}
