namespace DunorGames.Infrastructure.Persistence.Entities;

public sealed class GameSystemEntity
{
    public string Code { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public int SortOrder { get; init; }
}
