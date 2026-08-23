using System.Text.Json.Nodes;

namespace DunorGames.Domain.Statblocks;

public sealed record Statblock(
    Guid Id,
    string SchemaVersion,
    StatblockSystem System,
    StatblockIdentity Identity,
    string? Description,
    IReadOnlyList<string> Tags,
    StatblockSource Source,
    StatblockMetadata Metadata,
    JsonObject SystemData);

public sealed record StatblockIdentity(
    string Name,
    string? Subtitle = null,
    IReadOnlyList<string>? Aliases = null);

public sealed record StatblockSource(
    StatblockOrigin Origin,
    StatblockSystem? SourceSystem = null,
    Guid? SourceStatblockId = null,
    string? Reference = null);

public enum StatblockOrigin
{
    Manual,
    Imported,
    Converted
}

public sealed record StatblockMetadata(
    Guid OwnerId,
    StatblockStatus Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Notes = null);

public enum StatblockStatus
{
    Draft,
    Published,
    Archived
}
