using DunorGames.Data.Persistence.Entities;

namespace DunorGames.Data.Persistence;

public static class DaggerheartReferenceValues
{
    public static readonly ReferenceValueEntity[] All =
        Values("tier", ["1", "2", "3", "4"], code => $"Tier {code}")
            .Concat(Values("adversaryType", ["bruiser", "horde", "leader", "minion", "ranged", "skulk", "social", "solo", "standard", "support"], Title))
            .Concat(Values("attackRange", ["melee", "very close", "close", "far", "very far"], Title))
            .Concat(Values("damageType", ["physical", "magical"], Title))
            .Concat(Values("damageDie", ["4", "6", "8", "10", "12", "20"], code => $"d{code}"))
            .ToArray();

    private static IEnumerable<ReferenceValueEntity> Values(
        string category,
        string[] codes,
        Func<string, string> displayName) =>
        codes.Select((code, index) => new ReferenceValueEntity
        {
            SystemCode = "daggerheart",
            Category = category,
            Code = code,
            DisplayName = displayName(code),
            SortOrder = index + 1,
            IsActive = true
        });

    private static string Title(string code) =>
        string.Join(' ', code.Split(' ').Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
}
