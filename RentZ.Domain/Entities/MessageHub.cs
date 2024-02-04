using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class MessageHub
{
    public int Id { get; set; }

    [ForeignKey(nameof(Sender))]
    public Guid SenderId { get; set; }

    [ForeignKey(nameof(Receiver))]
    public Guid ReceiverId { get; set; }

    public virtual Client Sender { get; set; } = null!;
    public virtual Client Receiver { get; set; } = null!;
}