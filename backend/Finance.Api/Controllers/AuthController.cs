using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Finance.Api.Controllers;

public sealed record LoginInput(string Username, string Password);
public sealed class FinanceUser
{
    public string Username { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public string[] Roles { get; set; } = [];
    public string[] Shops { get; set; } = [];
}

[ApiController, Route("api/auth")]
public sealed class AuthController(IConfiguration config, IWebHostEnvironment environment) : ControllerBase
{
    [AllowAnonymous, HttpPost("login")]
    public async Task<IActionResult> Login(LoginInput input)
    {
        var users = config.GetSection("FinanceUsers").Get<FinanceUser[]>() ?? [];
        if (environment.IsDevelopment() && users.Length == 0)
        {
            var hasher = new PasswordHasher<FinanceUser>();
            users = new[] { "maker", "checker", "controller", "admin", "connector" }.Select(name =>
            {
                var user = new FinanceUser { Username = name, Roles = [name == "connector" ? "Integration" : char.ToUpperInvariant(name[0]) + name[1..]], Shops = name == "admin" ? ["TIKTOK-HCM", "SHOPEE-HCM"] : ["TIKTOK-HCM"] };
                user.PasswordHash = hasher.HashPassword(user, "FinanceDemo!2026");
                return user;
            }).ToArray();
        }
        var account = users.SingleOrDefault(u => string.Equals(u.Username, input.Username, StringComparison.OrdinalIgnoreCase));
        if (account is null || new PasswordHasher<FinanceUser>().VerifyHashedPassword(account, account.PasswordHash, input.Password) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không đúng." });
        var claims = new List<Claim> { new(ClaimTypes.Name, account.Username) };
        claims.AddRange(account.Roles.Select(role => new Claim(ClaimTypes.Role, role)));
        claims.AddRange(account.Shops.Select(shop => new Claim("shop", shop.ToUpperInvariant())));
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));
        return Ok(new { username = account.Username, roles = account.Roles, shops = account.Shops });
    }
    [HttpGet("me")]
    public IActionResult Me() => Ok(new { username = User.Identity!.Name, roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value), shops = User.FindAll("shop").Select(c => c.Value) });
    [HttpPost("logout")]
    public async Task<IActionResult> Logout() { await HttpContext.SignOutAsync(); return NoContent(); }
}
