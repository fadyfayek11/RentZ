using Microsoft.EntityFrameworkCore;
using RentZ.DTO.Enums;
using RentZ.DTO.Response;
using RentZ.Infrastructure.Context;

namespace RentZ.Application.Services.Notification;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<BaseResponse<int?>> NotificationCount(string userId)
    {
        var notificationCount = await _context.Notifications.CountAsync(x => x.ClientId == Guid.Parse(userId));
        return new BaseResponse<int?>() { Code = ErrorCode.Success, Message = "Getting notification Count", Data = notificationCount };
    }
}