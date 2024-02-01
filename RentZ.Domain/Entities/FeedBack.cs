#nullable disable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class FeedBack
{
    [Key]
    public int Id { get; set; }
    public string Feedback { get; set; }

    [ForeignKey(nameof(Client))]
    public Guid OwnerId { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;
    public virtual Client Client { get; set; } = null!;
}