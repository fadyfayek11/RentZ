using Microsoft.EntityFrameworkCore;
using RentZ.DTO.Enums;
using RentZ.DTO.Notification;
using RentZ.DTO.Property;
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
        var notificationCount = await _context.Notifications.CountAsync(x => x.ReceiverId == Guid.Parse(userId) && !x.IsRead);
        
        var conversationCount = await _context.Conversations.CountAsync(x => (x.ReceiverId == Guid.Parse(userId) && !x.IsReadByReceiver) ||
                                                                             (x.SenderId == Guid.Parse(userId) && !x.IsReadBySender));
        
        return new BaseResponse<int?>() { Code = ErrorCode.Success, Message = "Getting notification Count", Data = notificationCount + conversationCount };
    }

    public async Task<BaseResponse<bool?>> ReadNotification(int id, string userId)
    {
        var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.Id == id && x.ReceiverId == Guid.Parse(userId));
       
        if (notification is null) return new BaseResponse<bool?>() { Code = ErrorCode.BadRequest, Message = "Can't find it", Data = false };

        notification.IsRead = true;
        
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
         
        return new BaseResponse<bool?>() { Code = ErrorCode.Success, Message = "Notification read successfully", Data = true };
    }

    public async Task<BaseResponse<bool?>> AddNotification(AddNotification request)
    {
        if (request.Type == NotificationTypes.Message)
        {
            var getLastNotification =
                await _context.Notifications.FirstOrDefaultAsync(x => x.ReceiverId == Guid.Parse(request.ReceiverId) && x.SenderId == Guid.Parse(request.SenderId) && !x.IsRead);
            
            if (getLastNotification is not null)
            {
                return new BaseResponse<bool?>() { Code = ErrorCode.Success, Message = "Notification added successfully", Data = true };
            }

            request.Content = await _context.Users.Where(x => x.Id == Guid.Parse(request.SenderId))
                .Select(x => x.DisplayName).FirstOrDefaultAsync();
        }
        var notification = new Domain.Entities.Notification
        {
            Type = request.Type,
            Title = request.Title,
            Content = request.Content,
            LinkId = request.LinkId,
            CreatedAt = DateTime.Now,
            IsRead = false,
            ReceiverId = Guid.Parse(request.ReceiverId),
            SenderId = Guid.Parse(request.SenderId),
        };
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();

        return new BaseResponse<bool?>() { Code = ErrorCode.Success, Message = "Notification added successfully", Data = true };
    }

    public async Task<BaseResponse<PagedResult<GetNotifications?>>> NotificationsList(Pagination pagination, string uId)
    {
        var notifications = await _context.Notifications
            .Where(x => x.ReceiverId == Guid.Parse(uId))
            .Skip((pagination.PageIndex - 1) * pagination.PageSize)
            .Take(pagination.PageSize).OrderByDescending(x => x.CreatedAt).ToListAsync();

        var totalCount = await _context.Notifications.CountAsync();
        var response = notifications.Select(x => new GetNotifications
        {
            Id = x.Id,
            Type = x.Type,
            Title = x.Title,
            Content = x.Content,
            LinkId = x.LinkId,
            CreatedAt = x.CreatedAt,
            IsRead = x.IsRead
        }).ToList();

        return new BaseResponse<PagedResult<GetNotifications?>>() { Code = ErrorCode.Success, Message = "Getting list of notifications", Data = new PagedResult<GetNotifications?>() { Items = response, TotalCount = totalCount } };
    }
}