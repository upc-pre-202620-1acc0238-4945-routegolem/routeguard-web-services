using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.Shared.Application.Model;

namespace RouteGuard.Platform.NotificationsCommunication.Application.CommandServices;

public interface INotificationCommandService
{
    Task<Result<Notification>> Handle(CreateNotificationCommand command, CancellationToken cancellationToken = default);
    Task<Result<Notification>> Handle(DispatchNotificationCommand command, CancellationToken cancellationToken = default);
    Task<Result<Notification>> Handle(MarkNotificationDeliveredCommand command, CancellationToken cancellationToken = default);
    Task<Result<Notification>> Handle(TriggerAlertCommand command, CancellationToken cancellationToken = default);
    Task<Result<Notification>> Handle(PublishAnnouncementCommand command, CancellationToken cancellationToken = default);
}
