#nullable disable
using RentZ.DTO.Enums;

namespace RentZ.DTO.Notification;

public class GetNotifications
{
    public int Id { get; set; }

    public NotificationTypes Type { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int? LinkId { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}