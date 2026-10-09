using Microsoft.AspNetCore.Authorization;
using RouteGuard.Platform.Shared.Interfaces.Rest.Security;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using RouteGuard.Platform.NotificationsCommunication.Application.CommandServices;
using RouteGuard.Platform.NotificationsCommunication.Application.QueryServices;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Commands;
using RouteGuard.Platform.NotificationsCommunication.Domain.Model.Queries;
using RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Resources;
using RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest.Transform;
using RouteGuard.Platform.Shared.Interfaces.Rest.ProblemDetails;
using Swashbuckle.AspNetCore.Annotations;

namespace RouteGuard.Platform.NotificationsCommunication.Interfaces.Rest;

[ApiController]
[Route("api/v1/[controller]")]
[Produces(MediaTypeNames.Application.Json)]
[SwaggerTag("Notification and communication endpoints.")]
public class NotificationsController(
    INotificationCommandService commandService,
    INotificationQueryService queryService,
    ProblemDetailsFactory problemDetailsFactory,
    CallerContext caller) : ControllerBase
{
    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationResource resource, CancellationToken cancellationToken)
    {
        var command = new CreateNotificationCommand(resource.OrganizationId, resource.ParentId, resource.TripId, resource.Category, resource.Message);
        var result = await commandService.Handle(command, cancellationToken);
        
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            notification => CreatedAtAction(nameof(GetNotificationById), new { notificationId = notification.Id.Identifier }, 
                NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpGet("{notificationId:guid}")]
    public async Task<IActionResult> GetNotificationById(Guid notificationId, CancellationToken cancellationToken)
    {
        var notification = await queryService.Handle(new GetNotificationByIdQuery(notificationId), cancellationToken);
        if (notification is null) return NotFound();
        return Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpGet]
    public async Task<IActionResult> GetNotifications([FromQuery] Guid? parentId, CancellationToken cancellationToken)
    {
        IEnumerable<RouteGuard.Platform.NotificationsCommunication.Domain.Model.Aggregates.Notification> notifications;
        // A parent only receives their own notifications, whatever filter they send.
        if (caller.IsParent)
        {
            var own = await caller.GetParentIdAsync(cancellationToken);
            if (own is null || (parentId.HasValue && parentId != own)) return Forbid();
            parentId = own;
        }

        if (parentId.HasValue)
        {
            notifications = await queryService.Handle(new GetNotificationsByParentIdQuery(parentId.Value), cancellationToken);
        }
        else
        {
            notifications = await queryService.Handle(new GetAllNotificationsQuery(), cancellationToken);
        }
        
        var resources = notifications.Select(NotificationResourceFromEntityAssembler.ToResourceFromEntity);
        return Ok(resources);
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{notificationId:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid notificationId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new DispatchNotificationCommand(notificationId), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            notification => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification)));
    }

    [Authorize(Roles = AppRoles.Any)]
    [HttpPost("{notificationId:guid}/delivered")]
    public async Task<IActionResult> MarkDelivered(Guid notificationId, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new MarkNotificationDeliveredCommand(notificationId), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            notification => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{notificationId:guid}/alerts")]
    public async Task<IActionResult> TriggerAlert(Guid notificationId, [FromBody] TriggerAlertResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new TriggerAlertCommand(notificationId, resource.Panic), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            notification => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification)));
    }

    [Authorize(Roles = AppRoles.Admin)]
    [HttpPost("{notificationId:guid}/announcements")]
    public async Task<IActionResult> PublishAnnouncement(Guid notificationId, [FromBody] PublishAnnouncementResource resource, CancellationToken cancellationToken)
    {
        var result = await commandService.Handle(new PublishAnnouncementCommand(notificationId, resource.RouteId, resource.Message), cancellationToken);
        return NotificationActionResultAssembler.ToActionResult(this, result, problemDetailsFactory,
            notification => Ok(NotificationResourceFromEntityAssembler.ToResourceFromEntity(notification)));
    }
}
