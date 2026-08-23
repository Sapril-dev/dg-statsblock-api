namespace DunorGames.Infrastructure.Persistence.Entities;

public sealed class StatblockTagEntity
{
    public Guid StatblockId { get; init; }

    public string Tag { get; init; } = string.Empty;

    public StatblockEntity Statblock { get; init; } = null!;
}
