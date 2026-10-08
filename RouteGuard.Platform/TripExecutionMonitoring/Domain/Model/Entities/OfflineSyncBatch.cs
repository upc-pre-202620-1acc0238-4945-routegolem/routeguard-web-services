using RouteGuard.Platform.Shared.Domain.Model.ValueObjects;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.ValueObjects;

namespace RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Entities;

/// <summary>A batch of records the driver saved without signal and synchronized later (table <c>offline_sync_batches</c>).</summary>
public class OfflineSyncBatch
{
    protected OfflineSyncBatch()
    {
        TripId = new TripId(Guid.Empty);
    }

    public OfflineSyncBatch(TripId tripId, int syncedRecordsCount, string rawPayload)
    {
        Id = Guid.NewGuid();
        TripId = tripId;
        SyncedRecordsCount = syncedRecordsCount;
        RawPayload = rawPayload;
        SyncedAt = DateTimeOffset.UtcNow;
        CreatedAt = SyncedAt;
    }

    public Guid Id { get; private set; }
    public TripId TripId { get; private set; }
    public int SyncedRecordsCount { get; private set; }
    public string RawPayload { get; private set; } = "{}";
    public DateTimeOffset SyncedAt { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
