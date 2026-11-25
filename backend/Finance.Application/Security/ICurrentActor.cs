namespace Finance.Application.Security;

public interface ICurrentActor
{
    string UserName { get; }
    IReadOnlyList<string> Shops { get; }
    IReadOnlyList<string> Roles { get; }
    void RequireShop(string shopCode);
    void RequireRole(params string[] roles);
    void RequireApproval(decimal amount, string operation) => RequireRole("Checker", "Controller");
}

public static class FinanceRules
{
    public static DateOnly Month(DateOnly date) => date == default ? throw new ArgumentException("Kỳ là bắt buộc.") : new(date.Year, date.Month, 1);
    public static string Scope(string value) => string.IsNullOrWhiteSpace(value) ? throw new ArgumentException("Scope là bắt buộc.") : value.Trim().ToUpperInvariant();
}
