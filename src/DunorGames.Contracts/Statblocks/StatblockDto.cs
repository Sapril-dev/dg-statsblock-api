using System.Text.Json.Nodes;

namespace DunorGames.Contracts.Statblocks;

public sealed record StatblockDto(
    string SchemaVersion,
    Guid Id,
    StatblockSystemDto System,
    StatblockIdentityDto Identity,
    string? Description,
    IReadOnlyList<string> Tags,
    StatblockSourceDto Source,
    StatblockMetadataDto Metadata,
    JsonObject SystemData);

public enum StatblockSystemDto
{
    DungeonsAndDragons,
    TalesOfTheValiant,
    Daggerheart,
    DrawSteel,
    Dc20
}

public sealed record StatblockIdentityDto(
    string Name,
    string? Subtitle = null,
    IReadOnlyList<string>? Aliases = null);

public sealed record StatblockSourceDto(
    StatblockOriginDto Origin,
    StatblockSystemDto? SourceSystem = null,
    Guid? SourceStatblockId = null,
    string? Reference = null);

public enum StatblockOriginDto
{
    Manual,
    Imported,
    Converted
}

public sealed record StatblockMetadataDto(
    Guid OwnerId,
    StatblockStatusDto Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Notes = null);

public enum StatblockStatusDto
{
    Draft,
    Published,
    Archived
}
