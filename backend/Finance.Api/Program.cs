using Finance.Application.Imports;
using Finance.Application.Exceptions;
using Finance.Application.Reconciliation;
using Finance.Application.Ledger;
using Finance.Application.Classification;
using Finance.Infrastructure.Persistence;
using System.Text.Json.Serialization;
using Finance.Application.Security;
using Finance.Api.Security;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Finance.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentActor, CurrentActor>();
builder.Services.AddScoped<FinanceAccessFilter>();
builder.Services.AddControllers(options => options.Filters.AddService<FinanceAccessFilter>()).AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.Cookie.Name = "finance.session";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.ExpireTimeSpan = TimeSpan.FromHours(4);
    options.Events.OnRedirectToLogin = context => { context.Response.StatusCode = 401; return Task.CompletedTask; };
    options.Events.OnRedirectToAccessDenied = context => { context.Response.StatusCode = 403; return Task.CompletedTask; };
});
builder.Services.AddAuthorization(options => options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFinancePersistence(builder.Configuration.GetConnectionString("FinanceDb") ?? "Data Source=finance.db");
builder.Services.AddScoped<IImportBatchRepository, EfImportBatchRepository>();
builder.Services.AddScoped<ImportBatchService>();
builder.Services.AddScoped<IReconciliationRepository, EfReconciliationRepository>();
builder.Services.AddScoped<ReconciliationService>();
builder.Services.AddScoped<IFinanceExceptionRepository, EfFinanceExceptionRepository>();
builder.Services.AddScoped<ExceptionService>();
builder.Services.AddScoped<ILedgerRepository, EfLedgerRepository>();
builder.Services.AddScoped<LedgerService>();
builder.Services.AddScoped<IClassificationRepository, EfClassificationRepository>();
builder.Services.AddScoped<ClassificationService>();
builder.Services.AddScoped<FinanceExtensionService>();
builder.Services.AddCors(options => options.AddDefaultPolicy(policy => policy.WithOrigins("http://localhost:3000", "http://127.0.0.1:3000").AllowCredentials().AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.Use(async (context, next) =>
{
    try
    {
        if (context.Request.Path.StartsWithSegments("/api") && !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsOptions(context.Request.Method) && context.Request.Headers["X-Finance-Client"] != "web")
        { context.Response.StatusCode = 403; await context.Response.WriteAsJsonAsync(new { message = "Thiếu header X-Finance-Client." }); return; }
        await next();
    }
    catch (Exception error) when (error is ArgumentException or UnauthorizedAccessException or InvalidOperationException or DbUpdateException)
    {
        context.Response.StatusCode = error is UnauthorizedAccessException ? 403 : error is DbUpdateException or InvalidOperationException ? 409 : 400;
        await context.Response.WriteAsJsonAsync(new { message = error is DbUpdateException ? "Dữ liệu xung đột hoặc thao tác trùng. Tải lại trước khi thử lại." : error.Message });
    }
});
await app.Services.InitializeFinanceDatabaseAsync();
app.UseSwagger();
app.UseSwaggerUI();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
