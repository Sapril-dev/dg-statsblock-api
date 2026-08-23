using System.Text.Json;
using System.Text.Json.Serialization;
using DunorGames.Contracts.Statblocks;

public sealed class StatblockContractTests
{
    [Fact]
    public void CanonicalEnvelopeDeserializesDaggerheartStatblock()
    {
        const string json = """
        {
          "schemaVersion": "1.0",
          "id": "d965d7a9-13b8-41ae-9cb2-4f1d158c8d22",
          "system": "daggerheart",
          "identity": { "name": "Street Bandit" },
          "tags": ["minion"],
          "source": { "origin": "imported" },
          "metadata": {
            "ownerId": "00000000-0000-0000-0000-000000000001",
            "status": "draft",
            "createdAt": "2026-08-21T00:00:00Z",
            "updatedAt": "2026-08-21T00:00:00Z"
          },
          "systemData": { "tier": 1 }
        }
        """;

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));

        var statblock = JsonSerializer.Deserialize<StatblockDto>(json, options);

        Assert.NotNull(statblock);
        Assert.Equal(StatblockSystemDto.Daggerheart, statblock.System);
        Assert.Equal("Street Bandit", statblock.Identity.Name);
        Assert.Equal(1, statblock.SystemData["tier"]!.GetValue<int>());
    }
}
