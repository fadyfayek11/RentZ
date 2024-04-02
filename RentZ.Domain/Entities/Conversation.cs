using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class Conversation
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Sender))]
    public Guid SenderId { get; set; }

    [ForeignKey(nameof(Receiver))]
    public Guid ReceiverId { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
    public bool IsReadBySender { get; set; }
    public bool IsSenderOnline { get; set; }
    public bool IsReadByReceiver { get; set; }
    public bool IsReceiverOnline { get; set; }

    [ForeignKey(nameof(Property))]
    public int PropId { get; set; }

    public virtual Client Sender { get; set; } = null!;
    public virtual Client Receiver { get; set; } = null!;
    public virtual Property Property { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = null!;
}