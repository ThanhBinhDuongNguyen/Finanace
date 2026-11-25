using Finance.Application.Security;
using Finance.Domain.Ledger;
using Finance.Domain.Reconciliation;
using Finance.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Finance.Api.Controllers;

public sealed record ToleranceInput(string Currency, decimal Amount, DateOnly EffectiveFrom, DateOnly EffectiveTo, string Evidence);
public sealed record ReopenPeriodInput(string ShopCode, DateOnly Period, string Reason, string Evidence);

[ApiController, Route("api/finance/controls")]
public sealed class ControlsController(FinanceDbContext db, ICurrentActor actor) : ControllerBase
{
    [HttpGet("policies")]
    public async Task<IActionResult> Policies(CancellationToken ct) => Ok(await db.Set<TolerancePolicy>().ToListAsync(ct));
    [HttpPost("policies")]
    public async Task<IActionResult> AddPolicy(ToleranceInput input, CancellationToken ct)
    {
        actor.RequireRole("Admin");
        if (input.Amount < 0 || input.EffectiveFrom == default || input.EffectiveTo < input.EffectiveFrom || string.IsNullOrWhiteSpace(input.Evidence)) throw new ArgumentException("Policy cần dung sai không âm, khoảng hiệu lực và evidence.");
        var currency = FinanceRules.Scope(input.Currency);
        if (currency.Length != 3) throw new ArgumentException("Currency cần mã 3 ký tự.");
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        if (await db.Set<TolerancePolicy>().AnyAsync(p => p.Currency == currency && p.EffectiveFrom <= input.EffectiveTo && p.EffectiveTo >= input.EffectiveFrom, ct)) throw new InvalidOperationException("Policy chồng thời gian hiệu lực.");
        var policy = new TolerancePolicy { Currency = currency, Amount = input.Amount, EffectiveFrom = input.EffectiveFrom, EffectiveTo = input.EffectiveTo, Evidence = input.Evidence.Trim(), ApprovedBy = actor.UserName };
        db.Add(policy); await db.SaveChangesAsync(ct); await transaction.CommitAsync(ct); return Ok(policy);
    }
    [HttpPost("reopen-requests")]
    public async Task<IActionResult> ProposeReopen(ReopenPeriodInput input, CancellationToken ct)
    {
        actor.RequireRole("Admin"); actor.RequireShop(input.ShopCode);
        if (string.IsNullOrWhiteSpace(input.Reason) || string.IsNullOrWhiteSpace(input.Evidence)) throw new ArgumentException("Mở kỳ cần lý do và evidence.");
        var proposal = new PeriodReopenRequest { ShopCode = FinanceRules.Scope(input.ShopCode), Period = FinanceRules.Month(input.Period), ProposedBy = actor.UserName, Reason = input.Reason.Trim(), Evidence = input.Evidence.Trim() };
        db.Add(proposal); await db.SaveChangesAsync(ct); return Ok(proposal);
    }
    [HttpPost("reopen-requests/{id:guid}/approve")]
    public async Task<IActionResult> ApproveReopen(Guid id, CancellationToken ct)
    {
        actor.RequireRole("Controller");
        var proposal = await db.Set<PeriodReopenRequest>().SingleOrDefaultAsync(p => p.Id == id, ct) ?? throw new ArgumentException("Không tìm thấy yêu cầu mở kỳ.");
        if (proposal.ProposedBy == actor.UserName || proposal.ApprovedBy != null) throw new InvalidOperationException("Yêu cầu đã duyệt hoặc người tạo tự duyệt.");
        var end = proposal.Period.AddMonths(1);
        var locks = await db.ClosedPeriods.Where(p => p.ShopCode == proposal.ShopCode && p.Period >= proposal.Period && p.Period < end).ToListAsync(ct);
        db.ClosedPeriods.RemoveRange(locks); proposal.ApprovedBy = actor.UserName; await db.SaveChangesAsync(ct); return Ok(proposal);
    }
    [HttpGet("reopen-requests")]
    public async Task<IActionResult> ReopenRequests(CancellationToken ct) => Ok(await db.Set<PeriodReopenRequest>().ToListAsync(ct));
}
