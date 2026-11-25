using Finance.Application.Security;
using System.Security.Claims;

namespace Finance.Api.Security;

public sealed class CurrentActor(IHttpContextAccessor accessor, IConfiguration configuration, IWebHostEnvironment environment) : ICurrentActor
{
    public string UserName => accessor.HttpContext?.User.Identity?.Name ?? string.Empty;
    public IReadOnlyList<string> Shops => accessor.HttpContext?.User.FindAll("shop").Select(c => c.Value).ToArray() ?? [];
    public IReadOnlyList<string> Roles => accessor.HttpContext?.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToArray() ?? [];
    public void RequireShop(string shopCode)
    {
        if (!Shops.Contains(FinanceRules.Scope(shopCode))) throw new UnauthorizedAccessException("Không có quyền truy cập shop này.");
    }
    public void RequireRole(params string[] roles)
    {
        if (!roles.Any(Roles.Contains)) throw new UnauthorizedAccessException("Không có quyền thực hiện thao tác này.");
    }
    public void RequireApproval(decimal amount, string operation)
    {
        var limits = configuration.GetSection("FinanceApprovalLimits").Get<ApprovalLimit[]>() ?? [];
        if (environment.IsDevelopment() && limits.Length == 0) limits = [new("Checker", "*", 10_000_000), new("Controller", "*", 1_000_000_000)];
        if (!limits.Any(l => Roles.Contains(l.Role) && (l.Operation == operation || l.Operation == "*") && Math.Abs(amount) <= l.MaxAmount))
            throw new UnauthorizedAccessException("Số tiền vượt hạn mức phê duyệt của tài khoản cho nghiệp vụ này.");
    }
}

public sealed record ApprovalLimit(string Role, string Operation, decimal MaxAmount);
