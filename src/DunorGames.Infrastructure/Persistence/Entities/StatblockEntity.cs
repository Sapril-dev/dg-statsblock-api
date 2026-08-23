namespace DunorGames.Infrastructure.Persistence.Entities;

public sealed class StatblockEntity
{
    public Guid Id { get; init; }

    // A future ASP.NET Core Identity migration will use the same Guid key type.
    public Guid OwnerId { get; init; }

    public string SchemaVersion { get; init; } = string.Empty;

    public string SystemCode { get; init; } = string.Empty;

    public string Name { get; init; } = string.Empty;

    public string? Subtitle { get; init; }

    public string? Description { get; init; }

    public string Origin { get; init; } = string.Empty;

    public string? SourceSystemCode { get; init; }

    public Guid? SourceStatblockId { get; init; }

    public string? SourceReference { get; init; }

    public string Status { get; init; } = "draft";

    public string? Notes { get; init; }

    public string SystemDataJson { get; init; } = "{}";

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public byte[] RowVersion { get; init; } = [];

    public ICollection<StatblockTagEntity> Tags { get; init; } = new List<StatblockTagEntity>();
}
