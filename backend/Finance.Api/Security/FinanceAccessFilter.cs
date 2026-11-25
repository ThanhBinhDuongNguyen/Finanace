using Finance.Application.Security;
using Microsoft.AspNetCore.Mvc.Filters;
using Finance.Infrastructure.Persistence;
using Finance.Domain.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Security;

public sealed class FinanceAccessFilter(ICurrentActor actor, FinanceDbContext db) : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var request = context.HttpContext.Request;
        if (request.Path.StartsWithSegments("/api/finance"))
        {
            foreach (var arg in context.ActionArguments.Values.Where(value => value != null))
            {
                var shop = arg!.GetType().GetProperty("ShopCode")?.GetValue(arg) as string;
                if (shop is not null) actor.RequireShop(shop);
            }
            if (request.Method != "GET")
            {
                var path = request.Path.Value!;
                if (path.EndsWith("/reopen-requests")) actor.RequireRole("Admin");
                else if (path.Contains("/reopen-requests/") && path.EndsWith("/approve")) actor.RequireRole("Controller");
                else if (path.EndsWith("/approve")) actor.RequireRole("Checker", "Controller");
                else if (path.EndsWith("/review")) actor.RequireRole("Checker", "Controller");
                else if (path.EndsWith("/confirm") || path.EndsWith("/approve-closure")) actor.RequireRole("Checker");
                else if (path.Contains("/periods/")) actor.RequireRole("Controller");
                else if (path.Contains("/costs/policies")) actor.RequireRole("Controller", "Admin");
                else if (path.Contains("/import-batches")) actor.RequireRole("Maker", "Controller", "Admin", "Integration");
                else if (path.EndsWith("/mappings") || path.Contains("/policies")) actor.RequireRole("Admin");
                else actor.RequireRole("Maker", "Controller", "Admin");
            }
        }
        var result = await next();
        if (result.Exception is null && request.Path.StartsWithSegments("/api/finance") && request.Method != "GET" && !request.Path.Value!.EndsWith("/preview") && (result.Result as ObjectResult)?.StatusCode is not >= 400)
        {
            var shop = context.ActionArguments.Values.Select(v => v?.GetType().GetProperty("ShopCode")?.GetValue(v) as string).FirstOrDefault(v => v != null);
            var id = context.ActionArguments.TryGetValue("id", out var resource) ? resource as Guid? : null;
            if (shop is null && id is not null)
                shop = (await db.FinanceExceptions.FindAsync(id))?.ShopCode ?? (await db.JournalEntries.FindAsync(id))?.ShopCode;
            if (shop is null && id is not null)
            {
                var reconciliation = await db.ReconciliationCases.FindAsync(id);
                if (reconciliation != null) shop = (await db.Payouts.FindAsync(reconciliation.PayoutId))?.ShopCode;
            }
            if (shop is not null)
            {
                db.Add(new FinanceExtensionAudit { ShopCode = FinanceRules.Scope(shop), SourceId = id ?? Guid.Empty, Action = request.Path.Value!, Actor = actor.UserName, Evidence = "API decision recorded" });
                await db.SaveChangesAsync(context.HttpContext.RequestAborted);
            }
        }
    }
}
