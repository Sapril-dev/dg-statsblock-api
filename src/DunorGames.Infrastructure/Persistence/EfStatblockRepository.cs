using System.Text.Json;
using System.Text.Json.Nodes;
using DunorGames.Domain.Statblocks;
using DunorGames.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace DunorGames.Infrastructure.Persistence;

public sealed class EfStatblockRepository(DunorGamesDbContext dbContext)
    : IStatblockRepository
{
    public async Task<IReadOnlyList<VersionedStatblock>> ListAsync(
        Guid ownerId,
        StatblockRepositoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var entities = dbContext.Statblocks
            .AsNoTracking()
            .Include(statblock => statblock.Tags)
            .Where(statblock => statblock.OwnerId == ownerId);

        if (query.System is not null)
        {
            var system = SystemCode(query.System.Value);
            entities = entities.Where(statblock => statblock.SystemCode == system);
        }

        if (query.Status is not null)
        {
            var status = StatusCode(query.Status.Value);
            entities = entities.Where(statblock => statblock.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tag = query.Tag.Trim();
            entities = entities.Where(statblock =>
                statblock.Tags.Any(candidate => candidate.Tag == tag));
        }

        if (!string.IsNullOrWhiteSpace(query.Text))
        {
            var text = query.Text.Trim();
            entities = entities.Where(statblock =>
                statblock.Name.Contains(text) ||
                (statblock.Subtitle != null && statblock.Subtitle.Contains(text)) ||
                (statblock.Description != null && statblock.Description.Contains(text)));
        }

        if (query.BeforeUpdatedAt is not null && query.BeforeId is not null)
        {
            var updatedAt = query.BeforeUpdatedAt.Value;
            var id = query.BeforeId.Value;
            entities = entities.Where(statblock =>
                statblock.UpdatedAt < updatedAt ||
                (statblock.UpdatedAt == updatedAt && statblock.Id.CompareTo(id) < 0));
        }

        var page = await entities
            .OrderByDescending(statblock => statblock.UpdatedAt)
            .ThenByDescending(statblock => statblock.Id)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        return page.Select(ToVersioned).ToArray();
    }

    public async Task<VersionedStatblock?> GetAsync(
        Guid ownerId,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Statblocks
            .AsNoTracking()
            .Include(statblock => statblock.Tags)
            .SingleOrDefaultAsync(
                statblock => statblock.OwnerId == ownerId && statblock.Id == id,
                cancellationToken);

        return entity is null ? null : ToVersioned(entity);
    }

    public async Task<VersionedStatblock> CreateAsync(
        Statblock statblock,
        CancellationToken cancellationToken = default)
    {
        var entity = ToEntity(statblock);
        dbContext.Statblocks.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);
        return ToVersioned(entity);
    }

    public async Task<StatblockRepositoryUpdateResult> UpdateAsync(
        Statblock statblock,
        byte[] expectedVersion,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Statblocks
            .Include(candidate => candidate.Tags)
            .SingleOrDefaultAsync(
                candidate =>
                    candidate.OwnerId == statblock.Metadata.OwnerId &&
                    candidate.Id == statblock.Id,
                cancellationToken);

        if (entity is null)
        {
            return new StatblockRepositoryUpdateResult(
                StatblockRepositoryUpdateResultKind.NotFound);
        }

        if (!entity.RowVersion.SequenceEqual(expectedVersion))
        {
            return new StatblockRepositoryUpdateResult(
                StatblockRepositoryUpdateResultKind.PreconditionFailed);
        }

        Apply(statblock, entity);
        dbContext.Entry(entity)
            .Property(candidate => candidate.RowVersion)
            .OriginalValue = expectedVersion;

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return new StatblockRepositoryUpdateResult(
                StatblockRepositoryUpdateResultKind.PreconditionFailed);
        }

        return new StatblockRepositoryUpdateResult(
            StatblockRepositoryUpdateResultKind.Updated,
            ToVersioned(entity));
    }

    public async Task<StatblockRepositoryDeleteResult> DeleteAsync(
        Guid ownerId,
        Guid id,
        byte[] expectedVersion,
        CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.Statblocks.SingleOrDefaultAsync(
            candidate => candidate.OwnerId == ownerId && candidate.Id == id,
            cancellationToken);

        if (entity is null)
        {
            return StatblockRepositoryDeleteResult.NotFound;
        }

        if (!entity.RowVersion.SequenceEqual(expectedVersion))
        {
            return StatblockRepositoryDeleteResult.PreconditionFailed;
        }

        dbContext.Entry(entity)
            .Property(candidate => candidate.RowVersion)
            .OriginalValue = expectedVersion;
        dbContext.Statblocks.Remove(entity);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            return StatblockRepositoryDeleteResult.Deleted;
        }
        catch (DbUpdateConcurrencyException)
        {
            return StatblockRepositoryDeleteResult.PreconditionFailed;
        }
    }

    private static StatblockEntity ToEntity(Statblock statblock)
    {
        var entity = new StatblockEntity
        {
            Id = statblock.Id,
            OwnerId = statblock.Metadata.OwnerId,
            CreatedAt = statblock.Metadata.CreatedAt
        };
        Apply(statblock, entity);
        return entity;
    }

    private static void Apply(Statblock statblock, StatblockEntity entity)
    {
        entity.SchemaVersion = statblock.SchemaVersion;
        entity.SystemCode = SystemCode(statblock.System);
        entity.Name = statblock.Identity.Name;
        entity.Subtitle = statblock.Identity.Subtitle;
        entity.AliasesJson = SerializeAliases(statblock.Identity.Aliases);
        entity.Description = statblock.Description;
        entity.Origin = OriginCode(statblock.Source.Origin);
        entity.SourceSystemCode = statblock.Source.SourceSystem is null
            ? null
            : SystemCode(statblock.Source.SourceSystem.Value);
        entity.SourceStatblockId = statblock.Source.SourceStatblockId;
        entity.SourceReference = statblock.Source.Reference;
        entity.Status = StatusCode(statblock.Metadata.Status);
        entity.Notes = statblock.Metadata.Notes;
        entity.SystemDataJson = statblock.SystemData.ToJsonString();
        entity.UpdatedAt = statblock.Metadata.UpdatedAt;

        entity.Tags.Clear();
        foreach (var tag in NormalizeTags(statblock.Tags))
        {
            entity.Tags.Add(new StatblockTagEntity
            {
                StatblockId = entity.Id,
                Tag = tag,
                Statblock = entity
            });
        }
    }

    private static VersionedStatblock ToVersioned(StatblockEntity entity) =>
        new(
            new Statblock(
                entity.Id,
                entity.SchemaVersion,
                ParseSystem(entity.SystemCode),
                new StatblockIdentity(
                    entity.Name,
                    entity.Subtitle,
                    ParseAliases(entity.AliasesJson)),
                entity.Description,
                entity.Tags
                    .Select(tag => tag.Tag)
                    .Order(StringComparer.OrdinalIgnoreCase)
                    .ToArray(),
                new StatblockSource(
                    ParseOrigin(entity.Origin),
                    entity.SourceSystemCode is null
                        ? null
                        : ParseSystem(entity.SourceSystemCode),
                    entity.SourceStatblockId,
                    entity.SourceReference),
                new StatblockMetadata(
                    entity.OwnerId,
                    ParseStatus(entity.Status),
                    entity.CreatedAt,
                    entity.UpdatedAt,
                    entity.Notes),
                JsonNode.Parse(entity.SystemDataJson)!.AsObject()),
            entity.RowVersion.ToArray());

    private static string? SerializeAliases(IReadOnlyList<string>? aliases)
    {
        var normalized = aliases?
            .Select(alias => alias.Trim())
            .Where(alias => alias.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return normalized is { Length: > 0 }
            ? JsonSerializer.Serialize(normalized)
            : null;
    }

    private static IReadOnlyList<string>? ParseAliases(string? aliasesJson) =>
        string.IsNullOrWhiteSpace(aliasesJson)
            ? null
            : JsonSerializer.Deserialize<string[]>(aliasesJson);

    private static IReadOnlyList<string> NormalizeTags(IReadOnlyList<string> tags) =>
        tags
            .Select(tag => tag.Trim())
            .Where(tag => tag.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static string SystemCode(StatblockSystem system) => system switch
    {
        StatblockSystem.DungeonsAndDragons => "dungeonsAndDragons",
        StatblockSystem.TalesOfTheValiant => "talesOfTheValiant",
        StatblockSystem.Daggerheart => "daggerheart",
        StatblockSystem.DrawSteel => "drawSteel",
        StatblockSystem.Dc20 => "dc20",
        _ => throw new ArgumentOutOfRangeException(nameof(system))
    };

    private static StatblockSystem ParseSystem(string system) => system switch
    {
        "dungeonsAndDragons" => StatblockSystem.DungeonsAndDragons,
        "talesOfTheValiant" => StatblockSystem.TalesOfTheValiant,
        "daggerheart" => StatblockSystem.Daggerheart,
        "drawSteel" => StatblockSystem.DrawSteel,
        "dc20" => StatblockSystem.Dc20,
        _ => throw new InvalidOperationException($"Système inconnu : {system}.")
    };

    private static string OriginCode(StatblockOrigin origin) =>
        origin.ToString().ToLowerInvariant();

    private static StatblockOrigin ParseOrigin(string origin) =>
        Enum.Parse<StatblockOrigin>(origin, ignoreCase: true);

    private static string StatusCode(StatblockStatus status) =>
        status.ToString().ToLowerInvariant();

    private static StatblockStatus ParseStatus(string status) =>
        Enum.Parse<StatblockStatus>(status, ignoreCase: true);
}
