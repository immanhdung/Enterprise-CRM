using EnterpriseCRM.Application.Common.Interfaces;
using EnterpriseCRM.Application.Features.SystemAdministration;
using EnterpriseCRM.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseCRM.Infrastructure.Services;

public class NotificationCenterService : INotificationCenterService
{
    private readonly IApplicationDbContext _context;

    public NotificationCenterService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationSummaryDto> GetUserNotificationsAsync(
        Guid userId,
        bool? unreadOnly = null,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Notifications
            .AsNoTracking()
            .Where(n => n.RecipientUserId == userId);

        int totalCount = await query.CountAsync(cancellationToken);
        int unreadCount = await query.CountAsync(n => !n.IsRead, cancellationToken);

        if (unreadOnly == true)
        {
            query = query.Where(n => !n.IsRead);
        }

        var items = await query
            .OrderByDescending(n => n.CreatedAt)
            .Take(Math.Min(100, Math.Max(1, limit)))
            .Select(n => new NotificationDto(
                n.Id,
                n.Title,
                n.Message,
                n.Link,
                n.IsRead,
                n.Type,
                n.Category,
                n.Priority,
                n.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return new NotificationSummaryDto(totalCount, unreadCount, items);
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId, cancellationToken);

        if (notif == null) return false;

        notif.IsRead = true;
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> MarkAllAsReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var unread = await _context.Notifications
            .Where(n => n.RecipientUserId == userId && !n.IsRead)
            .ToListAsync(cancellationToken);

        foreach (var n in unread)
        {
            n.IsRead = true;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId, CancellationToken cancellationToken = default)
    {
        var notif = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientUserId == userId, cancellationToken);

        if (notif == null) return false;

        _context.Notifications.Remove(notif);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<NotificationDto> CreateAndDispatchAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        if (request.RecipientUserId.HasValue)
        {
            var notif = new Notification
            {
                Id = Guid.NewGuid(),
                RecipientUserId = request.RecipientUserId.Value,
                Title = request.Title.Trim(),
                Message = request.Message.Trim(),
                Link = request.Link?.Trim(),
                Type = request.Type,
                Category = request.Category,
                Priority = request.Priority,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _context.Notifications.Add(notif);
            await _context.SaveChangesAsync(cancellationToken);

            return new NotificationDto(
                notif.Id,
                notif.Title,
                notif.Message,
                notif.Link,
                notif.IsRead,
                notif.Type,
                notif.Category,
                notif.Priority,
                notif.CreatedAt
            );
        }
        else
        {
            // Broadcast to all active users
            var activeUserIds = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => u.Id)
                .ToListAsync(cancellationToken);

            var list = new List<Notification>();
            foreach (var uId in activeUserIds)
            {
                list.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    RecipientUserId = uId,
                    Title = request.Title.Trim(),
                    Message = request.Message.Trim(),
                    Link = request.Link?.Trim(),
                    Type = request.Type,
                    Category = request.Category,
                    Priority = request.Priority,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                });
            }

            _context.Notifications.AddRange(list);
            await _context.SaveChangesAsync(cancellationToken);

            var first = list.First();
            return new NotificationDto(
                first.Id,
                first.Title,
                first.Message,
                first.Link,
                first.IsRead,
                first.Type,
                first.Category,
                first.Priority,
                first.CreatedAt
            );
        }
    }
}
