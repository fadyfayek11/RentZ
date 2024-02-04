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

    [ForeignKey(nameof(Receiver))]
    public Guid ReceiverId { get; set; }
    
    [ForeignKey(nameof(Sender))]
    public Guid SenderId { get; set; }

    public virtual User Sender { get; set; } = null!;
    public virtual Client Receiver { get; set; } = null!;
}