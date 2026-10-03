namespace DunorGames.Data.Models;

public interface IStatblockRepository
{
    Task<IReadOnlyList<VersionedStatblock>> ListAsync(
        Guid ownerId,
        StatblockRepositoryQuery query,
        CancellationToken cancellationToken = default);

    Task<VersionedStatblock?> GetAsync(
        Guid ownerId,
        Guid id,
        CancellationToken cancellationToken = default);

    Task<VersionedStatblock> CreateAsync(
        Statblock statblock,
        CancellationToken cancellationToken = default);

    Task<StatblockRepositoryUpdateResult> UpdateAsync(
        Statblock statblock,
        byte[] expectedVersion,
        CancellationToken cancellationToken = default);

    Task<StatblockRepositoryDeleteResult> DeleteAsync(
        Guid ownerId,
        Guid id,
        byte[] expectedVersion,
        CancellationToken cancellationToken = default);
}

public sealed record StatblockRepositoryQuery(
    StatblockSystem? System,
    StatblockStatus? Status,
    string? Tag,
    string? Text,
    DateTimeOffset? BeforeUpdatedAt,
    Guid? BeforeId,
    int Limit);

public sealed record VersionedStatblock(
    Statblock Statblock,
    byte[] Version);

public enum StatblockRepositoryUpdateResultKind
{
    Updated,
    NotFound,
    PreconditionFailed
}

public sealed record StatblockRepositoryUpdateResult(
    StatblockRepositoryUpdateResultKind Kind,
    VersionedStatblock? Statblock = null);

public enum StatblockRepositoryDeleteResult
{
    Deleted,
    NotFound,
    PreconditionFailed
}
