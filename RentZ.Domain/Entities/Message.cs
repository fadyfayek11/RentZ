#nullable disable
using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class Message
{
    public int Id { get; set; }

    [ForeignKey(nameof(Sender))]
    public Guid SenderId { get; set; }
    
    [ForeignKey(nameof(Receiver))]
    public Guid ReceiverId { get; set; } 

    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.Now;
    public bool IsRead { get; set; }

    public virtual Client Sender { get; set; } = null!;
    public virtual Client Receiver { get; set; } = null!;

}