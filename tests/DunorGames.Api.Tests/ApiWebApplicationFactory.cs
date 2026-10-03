using System.Security.Cryptography;
using DunorGames.Data.Persistence;
using DunorGames.Data.Persistence.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"DunorGamesTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<DunorGamesDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<DunorGamesDbContext>>();
            services.AddSingleton<TestRowVersionInterceptor>();
            services.AddDbContext<DunorGamesDbContext>((provider, options) =>
            {
                options
                    .UseInMemoryDatabase(databaseName)
                    .AddInterceptors(provider.GetRequiredService<TestRowVersionInterceptor>());
            });
        });
    }
}

public sealed class TestRowVersionInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        ApplyVersions(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private static void ApplyVersions(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (var entry in context.ChangeTracker.Entries<StatblockEntity>()
                     .Where(entry =>
                         entry.State is EntityState.Added or EntityState.Modified))
        {
            entry.Property(statblock => statblock.RowVersion).CurrentValue =
                RandomNumberGenerator.GetBytes(8);
        }
    }
}
