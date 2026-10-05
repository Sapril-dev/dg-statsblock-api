using System.Net.Http.Json;
using DunorGames.Data.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public sealed class ReferenceValuesTests : IClassFixture<ApiWebApplicationFactory>
{
    private readonly ApiWebApplicationFactory factory;

    public ReferenceValuesTests(ApiWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task DaggerheartListsContainTheOfficialChoices()
    {
        using (var scope = factory.Services.CreateScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<DunorGamesDbContext>();
            await database.Database.EnsureCreatedAsync();
        }

        using var client = factory.CreateClient();
        var values = await client.GetFromJsonAsync<ReferenceValue[]>(
            "/api/v1/reference-values?system=daggerheart");

        Assert.NotNull(values);
        Assert.Equal(27, values.Length);
        Assert.Equal(["1", "2", "3", "4"],
            values.Where(value => value.Category == "tier").Select(value => value.Code));
        Assert.Equal(["4", "6", "8", "10", "12", "20"],
            values.Where(value => value.Category == "damageDie").Select(value => value.Code));
        Assert.Contains(values, value => value.Category == "damageType" && value.Code == "magical");
        Assert.Contains(values, value => value.Category == "adversaryType" && value.Code == "standard");
        Assert.Contains(values, value => value.Category == "attackRange" && value.Code == "very close");
    }

    private sealed record ReferenceValue(string Category, string Code, string DisplayName);
}
