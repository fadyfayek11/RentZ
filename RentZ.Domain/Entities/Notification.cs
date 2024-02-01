#nullable disable

using RentZ.DTO.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class Notification
{
    [Key]
    public int Id { get; set; }

    public NotificationTypes Type { get; set; }
    public string Title { get; set; }
    public string Content { get; set; }

    public int? LinkId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; } = false;

    [ForeignKey(nameof(Client))]
    public Guid? ClientId { get; set; }
    public virtual Client? Client { get; set; }
}