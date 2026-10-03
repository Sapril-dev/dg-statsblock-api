using DunorGames.Business.Statblocks;

namespace DunorGames.Business.Statblocks;

public interface IStatblockStore
{
    Task<StatblockStorePage> ListAsync(
        StatblockListQuery query,
        CancellationToken cancellationToken = default);
    Task<StoredStatblock?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default);
    Task<StoredStatblock> CreateAsync(
        CreateStatblockRequest request,
        CancellationToken cancellationToken = default);
    Task<StatblockStoreUpdateResult> UpdateAsync(
        Guid id,
        string? expectedETag,
        UpdateStatblockRequest request,
        CancellationToken cancellationToken = default);
    Task<StatblockStoreDeleteResult> DeleteAsync(
        Guid id,
        string? expectedETag,
        CancellationToken cancellationToken = default);
}

public sealed record StatblockListQuery(
    StatblockSystemDto? System,
    StatblockStatusDto? Status,
    string? Tag,
    string? Query,
    string? Cursor);

public sealed record StatblockStorePage(
    IReadOnlyList<StoredStatblock> Items,
    string? NextCursor);

public sealed record StoredStatblock(StatblockResponse Resource, byte[] Version)
{
    public string ETag => $"\"{Convert.ToBase64String(Version)}\"";
}

public enum StatblockStoreUpdateResultKind
{
    Updated,
    NotFound,
    PreconditionFailed
}

public sealed record StatblockStoreUpdateResult(
    StatblockStoreUpdateResultKind Kind,
    StoredStatblock? Statblock = null);

public enum StatblockStoreDeleteResult
{
    Deleted,
    NotFound,
    PreconditionFailed
}
