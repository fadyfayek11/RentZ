#nullable disable
using System.ComponentModel.DataAnnotations;

namespace RentZ.Domain.Entities;

public class Utility
{
    [Key]
    public int Id { get; set; }
    public string Name { get; set; }
    public string NameEn { get; set; }
    public bool IsActive { get; set; }
    public virtual ICollection<PropertyUtility> PropertyUtilities { get; set; }
}