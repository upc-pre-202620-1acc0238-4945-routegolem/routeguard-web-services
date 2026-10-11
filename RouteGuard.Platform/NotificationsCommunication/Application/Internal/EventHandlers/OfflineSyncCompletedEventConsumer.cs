using MassTransit;
using Microsoft.Extensions.Logging;
using RouteGuard.Platform.TripExecutionMonitoring.Domain.Model.Events;

namespace RouteGuard.Platform.NotificationsCommunication.Application.Internal.EventHandlers;

public class OfflineSyncCompletedEventConsumer : IConsumer<OfflineSyncCompletedEvent>
{
    private readonly ILogger<OfflineSyncCompletedEventConsumer> logger;

    public OfflineSyncCompletedEventConsumer(ILogger<OfflineSyncCompletedEventConsumer> logger)
    {
        this.logger = logger;
    }

    public Task Consume(ConsumeContext<OfflineSyncCompletedEvent> context)
    {
        var evt = context.Message;
        
        logger.LogInformation(
            "🟢 ¡MENSAJE RECIBIDO POR RABBITMQ! El viaje {TripId} acaba de sincronizar {Locations} ubicaciones y {Boardings} abordajes.",
            evt.TripId, evt.TotalLocationsSynced, evt.TotalBoardingsSynced);
        
        return Task.CompletedTask;
    }
}
