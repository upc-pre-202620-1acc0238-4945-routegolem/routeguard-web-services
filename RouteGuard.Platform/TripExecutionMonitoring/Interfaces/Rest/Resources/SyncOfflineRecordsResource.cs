using System.Text.Json;

namespace RouteGuard.Platform.TripExecutionMonitoring.Interfaces.Rest.Resources;

/// <summary>
///     Resource received from the mobile app containing a batch of offline events.
/// </summary>
public record SyncOfflineRecordsResource(
    int RecordsCount,
    JsonElement RawPayload
);