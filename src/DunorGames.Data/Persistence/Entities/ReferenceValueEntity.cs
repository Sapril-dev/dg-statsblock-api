namespace DunorGames.Data.Persistence.Entities;

public sealed class ReferenceValueEntity
{
    public string SystemCode { get; init; } = string.Empty;

    public string Category { get; init; } = string.Empty;

    public string Code { get; init; } = string.Empty;

    public string DisplayName { get; init; } = string.Empty;

    public int SortOrder { get; init; }

    public bool IsActive { get; init; } = true;
}
