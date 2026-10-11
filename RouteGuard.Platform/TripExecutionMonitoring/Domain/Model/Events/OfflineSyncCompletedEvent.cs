namespace RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Events;

/// <summary>
/// Evento que se dispara cuando un lote offline se ha guardado exitosamente en la BD.
/// </summary>
public record OfflineSyncCompletedEvent(
    Guid TripId, 
    int TotalLocationsSynced, 
    int TotalBoardingsSynced
);