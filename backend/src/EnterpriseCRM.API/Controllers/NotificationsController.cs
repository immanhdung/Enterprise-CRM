using EnterpriseCRM.API.Authorization;
using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Common.Models;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseCRM.API.Controllers;

[ApiController]
[Route("api/v1/notifications")]
[Authorize]
public class NotificationsController : BaseApiController
{
    private readonly INotificationCenterService _notificationService;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(INotificationCenterService notificationService, ICurrentUserService currentUserService)
    {
        _notificationService = notificationService;
        _currentUserService = currentUserService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<NotificationSummaryDto>>> GetNotifications([FromQuery] bool? unreadOnly, [FromQuery] int limit = 50, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var summary = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly, limit, ct);
        return OkResponse(summary);
    }

    [HttpPost("{id:guid}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _notificationService.MarkAsReadAsync(id, userId, ct);
        if (!result) return FailResponse<bool>("Notification not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "Marked as read.");
    }

    [HttpPost("read-all")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAllAsRead(CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _notificationService.MarkAllAsReadAsync(userId, ct);
        return OkResponse(result, "All notifications marked as read.");
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteNotification(Guid id, CancellationToken ct)
    {
        var userId = _currentUserService.UserId ?? Guid.Empty;
        var result = await _notificationService.DeleteNotificationAsync(id, userId, ct);
        if (!result) return FailResponse<bool>("Notification not found.", StatusCodes.Status404NotFound);
        return OkResponse(true, "Notification deleted.");
    }

    [HttpPost("dispatch")]
    [HasPermission(Permissions.NotificationsManage)]
    public async Task<ActionResult<ApiResponse<NotificationDto>>> DispatchNotification([FromBody] CreateNotificationRequest request, CancellationToken ct)
    {
        var result = await _notificationService.CreateAndDispatchAsync(request, ct);
        return OkResponse(result, "Notification dispatched successfully.");
    }
}
