using System.ComponentModel.DataAnnotations.Schema;

namespace RentZ.Domain.Entities;

public class FeedBack
{
    public int Id { get; set; }
    public string Feedback { get; set; }

    [ForeignKey(nameof(Client))]
    public Guid OwnerId { get; set; }

    public virtual Client Client { get; set; } = null!;
}