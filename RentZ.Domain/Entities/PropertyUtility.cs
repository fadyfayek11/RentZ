#nullable disable
namespace RentZ.Domain.Entities;

public class PropertyUtility
{
    public int PropertyId { get; set; }
    public int UtilityId { get; set; }

    public virtual Property Property { get; set; }
    public virtual Utility Utility { get; set; }
}