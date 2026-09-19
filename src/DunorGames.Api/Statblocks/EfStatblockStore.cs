using System.Text;
using DunorGames.Contracts.Statblocks;
using DunorGames.Domain.Statblocks;

namespace DunorGames.Api.Statblocks;

public sealed class EfStatblockStore : IStatblockStore
{
    private const int PageSize = 50;
    private static readonly Guid DefaultOwnerId =
        Guid.Parse("00000000-0000-0000-0000-000000000001");

    private readonly IStatblockRepository repository;
    private readonly Guid ownerId;

    public EfStatblockStore(
        IStatblockRepository repository,
        IConfiguration configuration)
    {
        this.repository = repository;
        ownerId = configuration.GetValue<Guid?>("Statblocks:DefaultOwnerId")
            ?? DefaultOwnerId;
    }

    public async Task<StatblockStorePage> ListAsync(
        StatblockListQuery query,
        CancellationToken cancellationToken = default)
    {
        var cursor = DecodeCursor(query.Cursor);
        var entities = await repository.ListAsync(
            ownerId,
            new StatblockRepositoryQuery(
                query.System is null ? null : ToDomain(query.System.Value),
                query.Status is null ? null : ToDomain(query.Status.Value),
                query.Tag,
                query.Query,
                cursor?.UpdatedAt,
                cursor?.Id,
                PageSize + 1),
            cancellationToken);

        var hasNextPage = entities.Count > PageSize;
        var items = entities
            .Take(PageSize)
            .Select(ToStored)
            .ToArray();
        var nextCursor = hasNextPage && items.Length > 0
            ? EncodeCursor(items[^1].Resource)
            : null;

        return new StatblockStorePage(items, nextCursor);
    }

    public async Task<StoredStatblock?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var statblock = await repository.GetAsync(ownerId, id, cancellationToken);
        return statblock is null ? null : ToStored(statblock);
    }

    public async Task<StoredStatblock> CreateAsync(
        CreateStatblockRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;
        var statblock = new Statblock(
            Guid.NewGuid(),
            request.SchemaVersion,
            ToDomain(request.System),
            ToDomain(request.Identity),
            request.Description,
            request.Tags,
            ToDomain(request.Source),
            new StatblockMetadata(
                ownerId,
                StatblockStatus.Draft,
                now,
                now,
                request.Notes),
            request.SystemData);

        return ToStored(await repository.CreateAsync(statblock, cancellationToken));
    }

    public async Task<StatblockStoreUpdateResult> UpdateAsync(
        Guid id,
        string? expectedETag,
        UpdateStatblockRequest request,
        CancellationToken cancellationToken = default)
    {
        var current = await repository.GetAsync(ownerId, id, cancellationToken);
        if (current is null)
        {
            return new StatblockStoreUpdateResult(
                StatblockStoreUpdateResultKind.NotFound);
        }

        var statblock = new Statblock(
            id,
            request.SchemaVersion,
            ToDomain(request.System),
            ToDomain(request.Identity),
            request.Description,
            request.Tags,
            ToDomain(request.Source),
            new StatblockMetadata(
                ownerId,
                ToDomain(request.Status),
                current.Statblock.Metadata.CreatedAt,
                DateTimeOffset.UtcNow,
                request.Notes),
            request.SystemData);

        var result = await repository.UpdateAsync(
            statblock,
            ParseETag(expectedETag),
            cancellationToken);

        return result.Kind switch
        {
            StatblockRepositoryUpdateResultKind.Updated =>
                new StatblockStoreUpdateResult(
                    StatblockStoreUpdateResultKind.Updated,
                    ToStored(result.Statblock!)),
            StatblockRepositoryUpdateResultKind.NotFound =>
                new StatblockStoreUpdateResult(
                    StatblockStoreUpdateResultKind.NotFound),
            _ => new StatblockStoreUpdateResult(
                StatblockStoreUpdateResultKind.PreconditionFailed)
        };
    }

    public async Task<StatblockStoreDeleteResult> DeleteAsync(
        Guid id,
        string? expectedETag,
        CancellationToken cancellationToken = default)
    {
        return await repository.DeleteAsync(
            ownerId,
            id,
            ParseETag(expectedETag),
            cancellationToken) switch
        {
            StatblockRepositoryDeleteResult.Deleted =>
                StatblockStoreDeleteResult.Deleted,
            StatblockRepositoryDeleteResult.NotFound =>
                StatblockStoreDeleteResult.NotFound,
            _ => StatblockStoreDeleteResult.PreconditionFailed
        };
    }

    private static StoredStatblock ToStored(VersionedStatblock value) =>
        new(
            new StatblockResponse(
                value.Statblock.Id,
                value.Statblock.SchemaVersion,
                ToContract(value.Statblock.System),
                new StatblockIdentityDto(
                    value.Statblock.Identity.Name,
                    value.Statblock.Identity.Subtitle,
                    value.Statblock.Identity.Aliases),
                value.Statblock.Description,
                value.Statblock.Tags,
                new StatblockSourceDto(
                    ToContract(value.Statblock.Source.Origin),
                    value.Statblock.Source.SourceSystem is null
                        ? null
                        : ToContract(value.Statblock.Source.SourceSystem.Value),
                    value.Statblock.Source.SourceStatblockId,
                    value.Statblock.Source.Reference),
                new StatblockReadMetadataDto(
                    ToContract(value.Statblock.Metadata.Status),
                    value.Statblock.Metadata.CreatedAt,
                    value.Statblock.Metadata.UpdatedAt,
                    value.Statblock.Metadata.Notes),
                value.Statblock.SystemData),
            value.Version);

    private static StatblockIdentity ToDomain(StatblockIdentityDto value) =>
        new(value.Name, value.Subtitle, value.Aliases);

    private static StatblockSource ToDomain(StatblockSourceDto value) =>
        new(
            ToDomain(value.Origin),
            value.SourceSystem is null ? null : ToDomain(value.SourceSystem.Value),
            value.SourceStatblockId,
            value.Reference);

    private static StatblockSystem ToDomain(StatblockSystemDto value) => value switch
    {
        StatblockSystemDto.DungeonsAndDragons => StatblockSystem.DungeonsAndDragons,
        StatblockSystemDto.TalesOfTheValiant => StatblockSystem.TalesOfTheValiant,
        StatblockSystemDto.Daggerheart => StatblockSystem.Daggerheart,
        StatblockSystemDto.DrawSteel => StatblockSystem.DrawSteel,
        StatblockSystemDto.Dc20 => StatblockSystem.Dc20,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static StatblockSystemDto ToContract(StatblockSystem value) => value switch
    {
        StatblockSystem.DungeonsAndDragons => StatblockSystemDto.DungeonsAndDragons,
        StatblockSystem.TalesOfTheValiant => StatblockSystemDto.TalesOfTheValiant,
        StatblockSystem.Daggerheart => StatblockSystemDto.Daggerheart,
        StatblockSystem.DrawSteel => StatblockSystemDto.DrawSteel,
        StatblockSystem.Dc20 => StatblockSystemDto.Dc20,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static StatblockOrigin ToDomain(StatblockOriginDto value) => value switch
    {
        StatblockOriginDto.Manual => StatblockOrigin.Manual,
        StatblockOriginDto.Imported => StatblockOrigin.Imported,
        StatblockOriginDto.Converted => StatblockOrigin.Converted,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static StatblockOriginDto ToContract(StatblockOrigin value) => value switch
    {
        StatblockOrigin.Manual => StatblockOriginDto.Manual,
        StatblockOrigin.Imported => StatblockOriginDto.Imported,
        StatblockOrigin.Converted => StatblockOriginDto.Converted,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static StatblockStatus ToDomain(StatblockStatusDto value) => value switch
    {
        StatblockStatusDto.Draft => StatblockStatus.Draft,
        StatblockStatusDto.Published => StatblockStatus.Published,
        StatblockStatusDto.Archived => StatblockStatus.Archived,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static StatblockStatusDto ToContract(StatblockStatus value) => value switch
    {
        StatblockStatus.Draft => StatblockStatusDto.Draft,
        StatblockStatus.Published => StatblockStatusDto.Published,
        StatblockStatus.Archived => StatblockStatusDto.Archived,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };

    private static byte[] ParseETag(string? etag)
    {
        if (string.IsNullOrWhiteSpace(etag) ||
            etag.StartsWith("W/", StringComparison.OrdinalIgnoreCase))
        {
            return [];
        }

        try
        {
            return Convert.FromBase64String(etag.Trim().Trim('"'));
        }
        catch (FormatException)
        {
            return [];
        }
    }

    private static string EncodeCursor(StatblockResponse statblock)
    {
        var value = $"{statblock.Metadata.UpdatedAt:O}|{statblock.Id:D}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static (DateTimeOffset UpdatedAt, Guid Id)? DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var encoded = cursor.Replace('-', '+').Replace('_', '/');
            encoded = encoded.PadRight(encoded.Length + ((4 - encoded.Length % 4) % 4), '=');
            var parts = Encoding.UTF8.GetString(Convert.FromBase64String(encoded)).Split('|');
            return parts.Length == 2 &&
                DateTimeOffset.TryParse(parts[0], out var updatedAt) &&
                Guid.TryParse(parts[1], out var id)
                    ? (updatedAt, id)
                    : null;
        }
        catch (FormatException)
        {
            return null;
        }
    }
}
