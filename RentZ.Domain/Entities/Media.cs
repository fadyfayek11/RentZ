#nullable disable
using System.ComponentModel.DataAnnotations;

namespace RentZ.Domain.Entities;

public class Media
{
    [Key]
    public int Id { get; set; }
    public string Reference { get; set; }
    public bool IsActive { get; set; }
}