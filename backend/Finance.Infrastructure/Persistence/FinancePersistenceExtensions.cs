using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Finance.Infrastructure.Persistence;

public static class FinancePersistenceExtensions
{
    public static IServiceCollection AddFinancePersistence(this IServiceCollection services, string connectionString) => services.AddDbContext<FinanceDbContext>(options => options.UseSqlite(connectionString));

    public static async Task InitializeFinanceDatabaseAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<FinanceDbContext>().Database.MigrateAsync();
    }
}
