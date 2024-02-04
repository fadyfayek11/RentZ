#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class Message
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(Conversation))]
    public int ConversationId { get; set; }

    public string Content { get; set; }
    public DateTime SentAt { get; set; } = DateTime.Now;

    public virtual Conversation Conversation { get; set; } = null!;

}