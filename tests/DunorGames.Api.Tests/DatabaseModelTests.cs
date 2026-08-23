using DunorGames.Infrastructure.Persistence;
using DunorGames.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

public sealed class DatabaseModelTests
{
    [Fact]
    public void InitialModelContainsApprovedTablesAndReferenceSystems()
    {
        var options = new DbContextOptionsBuilder<DunorGamesDbContext>()
            .UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=DunorGamesTest;Trusted_Connection=True")
            .Options;
        using var context = new DunorGamesDbContext(options);

        var model = context.GetService<IDesignTimeModel>().Model;
        var gameSystems = model.FindEntityType(typeof(GameSystemEntity));
        var statblocks = model.FindEntityType(typeof(StatblockEntity));
        var tags = model.FindEntityType(typeof(StatblockTagEntity));

        Assert.Equal("GameSystems", gameSystems!.GetTableName());
        Assert.Equal("Statblocks", statblocks!.GetTableName());
        Assert.Equal("StatblockTags", tags!.GetTableName());
        Assert.Equal(5, gameSystems!.GetSeedData().Count());
        Assert.NotNull(statblocks!.FindProperty(nameof(StatblockEntity.RowVersion)));
    }
}
