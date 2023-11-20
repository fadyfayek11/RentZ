namespace RentZ.Domain.Entities;

public class FavProperty
{
    public int PropertyId { get; set; }
    public Guid ClientId { get; set; }
    public bool IsActive { get; set; }

    public virtual Property Property { get; set; } = null!;
    public virtual Client Client { get; set; } = null!;
}