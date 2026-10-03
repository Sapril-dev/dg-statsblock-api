namespace DunorGames.Data.Persistence.Entities;

public sealed class StatblockEntity
{
    public Guid Id { get; init; }

    // A future ASP.NET Core Identity migration will use the same Guid key type.
    public Guid OwnerId { get; init; }

    public string SchemaVersion { get; set; } = string.Empty;

    public string SystemCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? Subtitle { get; set; }

    public string? AliasesJson { get; set; }

    public string? Description { get; set; }

    public string Origin { get; set; } = string.Empty;

    public string? SourceSystemCode { get; set; }

    public Guid? SourceStatblockId { get; set; }

    public string? SourceReference { get; set; }

    public string Status { get; set; } = "draft";

    public string? Notes { get; set; }

    public string SystemDataJson { get; set; } = "{}";

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = [];

    public ICollection<StatblockTagEntity> Tags { get; init; } = new List<StatblockTagEntity>();
}
