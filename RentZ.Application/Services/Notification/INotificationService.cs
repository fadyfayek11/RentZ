using RentZ.DTO.Response;

namespace RentZ.Application.Services.Notification;

public interface INotificationService
{
    Task<BaseResponse<int?>> NotificationCount(string userId);

}