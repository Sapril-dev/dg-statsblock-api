using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DunorGames.Infrastructure.Persistence;

/// <summary>
/// Enables creation of migrations without a deployed database. Production configuration is supplied at deployment.
/// </summary>
public sealed class DesignTimeDunorGamesDbContextFactory : IDesignTimeDbContextFactory<DunorGamesDbContext>
{
    private const string LocalDesignTimeConnection =
        "Server=(localdb)\\mssqllocaldb;Database=DunorGamesDesignTime;Trusted_Connection=True;TrustServerCertificate=True";

    public DunorGamesDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DUNORGAMES_CONNECTION_STRING")
            ?? LocalDesignTimeConnection;

        var options = new DbContextOptionsBuilder<DunorGamesDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new DunorGamesDbContext(options);
    }
}
