using RouteGuard.Platform.NotificationsCommunication.Application.CommandServices;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.NotificationsCommunication.Domain.Repositories;
using RouteGuard.Platform.Shared.Application.Model;
using RouteGuard.Platform.Shared.Domain.Repositories;

using Microsoft.EntityFrameworkCore;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Entities;
using RouteGuard.Platform.StakeholderAssetManagement.Domain.Model.Aggregates;
using RouteGuard.Platform.Shared.Infrastructure.Persistence.EntityFrameworkCore.Configuration;

namespace RouteGuard.Platform.NotificationsCommunication.Application.Internal.CommandServices;

public class NotificationCommandService(
    INotificationRepository notificationRepository,
    IUnitOfWork unitOfWork,
    AppDbContext dbContext) : INotificationCommandService
{
    public async Task<Result<Notification>> Handle(CreateNotificationCommand command, CancellationToken cancellationToken = default)
    {
        var notification = new Notification(command);
        
        await notificationRepository.AddAsync(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return Result<Notification>.Success(notification);
    }

    public async Task<Result<Notification>> Handle(DispatchNotificationCommand command, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.FindByIdAsync(command.NotificationId);
        if (notification is null) return Result<Notification>.Failure(NotificationError.NotificationNotFound, "Notification not found");
        
        var parent = await dbContext.Set<Parent>().FirstOrDefaultAsync(p => p.Id.Identifier == notification.ParentId.Identifier, cancellationToken);
        if (parent != null)
        {
            var deviceToken = await dbContext.Set<DeviceToken>().FirstOrDefaultAsync(d => d.UserId == parent.UserId && d.IsActive, cancellationToken);
            if (deviceToken != null && !string.IsNullOrWhiteSpace(deviceToken.Token))
            {
                var message = new FirebaseAdmin.Messaging.Message()
                {
                    Token = deviceToken.Token,
                    Notification = new FirebaseAdmin.Messaging.Notification()
                    {
                        Title = notification.Title,
                        Body = notification.Message.Content
                    }
                };
                
                if (!string.IsNullOrWhiteSpace(notification.DataPayload))
                {
                    try
                    {
                        message.Data = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(notification.DataPayload);
                    }
                    catch
                    {
                        message.Data = new Dictionary<string, string> { { "payload", notification.DataPayload } };
                    }
                }

                try
                {
                    await FirebaseAdmin.Messaging.FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
                }
                catch (FirebaseAdmin.Messaging.FirebaseMessagingException ex) when (ex.MessagingErrorCode == FirebaseAdmin.Messaging.MessagingErrorCode.Unregistered || ex.MessagingErrorCode == FirebaseAdmin.Messaging.MessagingErrorCode.InvalidArgument)
                {
                    deviceToken.Deactivate();
                    notification.Fail(ex.Message);
                }
                catch (Exception ex)
                {
                    notification.Fail(ex.Message);
                }
            }
        }
        
        notification.Dispatch();
        
        notificationRepository.Update(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return Result<Notification>.Success(notification);
    }

    public async Task<Result<Notification>> Handle(MarkNotificationDeliveredCommand command, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.FindByIdAsync(command.NotificationId);
        if (notification is null) return Result<Notification>.Failure(NotificationError.NotificationNotFound, "Notification not found");
        
        notification.MarkDelivered();
        
        notificationRepository.Update(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return Result<Notification>.Success(notification);
    }

    public async Task<Result<Notification>> Handle(TriggerAlertCommand command, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.FindByIdAsync(command.NotificationId);
        if (notification is null) return Result<Notification>.Failure(NotificationError.NotificationNotFound, "Notification not found");
        
        notification.AddAlert(command.Panic);
        
        notificationRepository.Update(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return Result<Notification>.Success(notification);
    }

    public async Task<Result<Notification>> Handle(PublishAnnouncementCommand command, CancellationToken cancellationToken = default)
    {
        var notification = await notificationRepository.FindByIdAsync(command.NotificationId);
        if (notification is null) return Result<Notification>.Failure(NotificationError.NotificationNotFound, "Notification not found");
        
        notification.AddAnnouncement(command.RouteId, command.Message);
        
        notificationRepository.Update(notification);
        await unitOfWork.CompleteAsync(cancellationToken);
        
        return Result<Notification>.Success(notification);
    }
}
