using RentZ.DTO.Notification;
using RentZ.DTO.Property;
using RentZ.DTO.Response;

namespace RentZ.Application.Services.Notification;

public interface INotificationService
{
    Task<BaseResponse<int?>> NotificationCount(string userId);
    Task<BaseResponse<bool?>> ReadNotification(int id, string userId);
    Task<BaseResponse<PagedResult<GetNotifications?>>> NotificationsList(Pagination pagination, string uId);

}