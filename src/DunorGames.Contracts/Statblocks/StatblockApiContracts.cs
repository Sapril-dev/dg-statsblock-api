using System.Text.Json.Nodes;

namespace DunorGames.Contracts.Statblocks;

/// <summary>
/// Resource returned by the public Statblocks API. Server ownership information is intentionally excluded.
/// </summary>
public sealed record StatblockResponse(
    Guid Id,
    string SchemaVersion,
    StatblockSystemDto System,
    StatblockIdentityDto Identity,
    string? Description,
    IReadOnlyList<string> Tags,
    StatblockSourceDto Source,
    StatblockReadMetadataDto Metadata,
    JsonObject SystemData);

public sealed record StatblockReadMetadataDto(
    StatblockStatusDto Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    string? Notes = null);

/// <summary>
/// Request used to create a Statblock. Identifiers, ownership and timestamps are assigned by the server.
/// </summary>
public sealed record CreateStatblockRequest(
    string SchemaVersion,
    StatblockSystemDto System,
    StatblockIdentityDto Identity,
    string? Description,
    IReadOnlyList<string> Tags,
    StatblockSourceDto Source,
    string? Notes,
    JsonObject SystemData);

/// <summary>
/// Complete mutable representation used by PUT. A client may set Draft, Published or Archived status.
/// </summary>
public sealed record UpdateStatblockRequest(
    string SchemaVersion,
    StatblockSystemDto System,
    StatblockIdentityDto Identity,
    string? Description,
    IReadOnlyList<string> Tags,
    StatblockSourceDto Source,
    StatblockStatusDto Status,
    string? Notes,
    JsonObject SystemData);

public sealed record StatblockSummaryResponse(
    Guid Id,
    StatblockSystemDto System,
    string Name,
    string? Subtitle,
    IReadOnlyList<string> Tags,
    StatblockStatusDto Status,
    DateTimeOffset UpdatedAt);

public sealed record CursorPageResponse<T>(
    IReadOnlyList<T> Items,
    string? NextCursor);

/// <summary>
/// RFC 9457-compatible problem response returned as application/problem+json.
/// </summary>
public sealed record ApiProblemResponse(
    string Type,
    string Title,
    int Status,
    string? Detail = null,
    string? Instance = null,
    IReadOnlyDictionary<string, string[]>? Errors = null);
